# Act 3 full rebuild — scratch + progress (started 2026-05-30, Opus)

## Why a rebuild
The `act3-D-verified` snapshot (and the other `act3-*` snapshots) were polluted by
**another project's tooling** sharing the same Postgres instance — `ActivityTypes` in
`act3-D-verified` had 429 columns incl. duplicate `PublishedVersion` + a stray
`ActivityTypes_BackupPre` table, main table 0 rows. User confirmed the corruption is
foreign and to ignore it. Decision: **restore the clean `after-act-2-replay` snapshot and
rebuild all of Act 3 via the visual builder + Playwright** (user choices: "Full Playwright
via the visual builder", "Full 3.A–3.I").

`after-act-2-replay` verified clean: ActivityTypes 20 cols / 20 rows / 0 dup cols;
Activities 0; CurriculumItemProgresses 0.

## Environment facts (verified against the clean DB)
- DB: `wombat_t002_verify`, user `wombat`, pw `3Uca!yptus#12`, host 127.0.0.1:5432.
- psql: `C:\Program Files\PostgreSQL\16\bin\psql.exe`.
- Dev server: start with `$env:ASPNETCORE_ENVIRONMENT='Development'` then
  `dotnet run --project src/Wombat.Web/Wombat.Web.csproj` (WITHOUT `--no-launch-profile`,
  else user-secrets don't load → tries user `postgres` → auth fail). URL http://localhost:5080.
- Snapshot tool: `tools\db-snapshot.ps1 {take|restore|list} <name>`.

## Real schema column names (NOT the scenario's guesses)
- `ActivityTypes`: Key, Name, Description, Scope, ScopeId, SchemaJson, WorkflowJson,
  CreditRulesJson, Version, IsActive, Staging{Schema,Workflow,CreditRules,DisplayFields}Json,
  StagingUpdatedByUserId/On, DisplayFieldsJson. (Publish promotes Staging→main, bumps Version.)
- `AssessorProfiles`: Id, UserId, Qualifications, InstitutionId, SpecialityId,
  SubSpecialityId, **TrainingCompletedOn** (date; NULL = in-training. No enum — T065.)
- `TraineeProfiles`: Id, UserId, CurriculumId, **ProgrammeStartDate**, ExpectedCompletionDate,
  IsActive. (`GetStage` anchors on ProgrammeStartDate; years=floor((today-start)/365); stage=years+1, cap StageCount/4.)
- `EntrustmentScales`: Id, Name, Description. Scale **2 = "Paed General Entrustment Scale"**.
- `EntrustmentLevels` (scale 2): 1 Observation only, 2 Direct supervision, 3 Indirect
  supervision, 4 Unsupervised, 5 Can supervise others.

## Cast (clean DB)
Trainees (all CurriculumId=2), stage @today 2026-05-30:
- Molefe `9ab51fa1…` start 2023-01-15 → **stage 4**
- Dlamini `0a98752d…` start 2024-01-15 → **stage 3**
- du Plessis `e430e208…` start 2025-01-15 → **stage 2**
- Mahlangu `666c9a12…` start 2026-01-15 → **stage 1**
- Ndlovu `63dc25b1…` start 2026-01-15 → **stage 1**
Assessors (inst 2, spec 2): Naidoo `f4b2005a…` trained, Patel `6b236065…` trained 2021-05-10,
Khumalo `0e67d819…` **in-training (NULL)**, Botha trained, Zulu trained.
Coordinator: Smit. Programme admin: Mbatha (InstitutionalAdmin).

## Activity types (all currently v1 title-only; need build+publish v2 via builder)
mini_cex_paed=11, cbd_paed=12, acat_paed=13, dops_paed=14, procedure_log_paed=15,
msf_paed=16, reflective_note_paed=17, journal_club_paed=18, research_output_paed=19,
teaching_session_paed=20.

## EPAs / curriculum items (item id | flat min | req | stage-min JSON)
- PAED-001 epa 2  | item 2  | 4 | 30 | {"1":2,"2":3,"3":4,"4":4}  (Mini-CEX, Dlamini stage3 min=4)
- PAED-010 epa 11 | item 11 | 4 | 10 | {"2":2,"3":3,"4":4}  ← **no stage-1 key** (DOPS, Mahlangu stage1)
- PAED-011 epa 12 | item 12 | 4 | 30 | {"1":2,"2":3,"3":4,"4":4}  (Procedure log, du Plessis stage2 min=3)

## Known findings to record as I go
- F-REBUILD-1: act3-* snapshots corrupted by another project (see above).
- 3.E: Patel seeded **trained** in Act 2 (TrainingCompletedOn 2021-05-10) — contradicts the
  Act-3 "Patel in-training" premise. Plan: NULL Patel's TrainingCompletedOn just before 3.E to
  exercise the in-training path, document the tweak. (Khumalo is the only actually-null one.)
- 3.E: PAED-010 has no stage-1 minimum entry → Mahlangu (stage 1) hits the GetMinimumLevelForStage
  fallback. Observe what the credit gate does (fallback to flat 4? to 0? to lowest stage?).

## Builder gotchas (from prior sessions)
- Set field **label before key**; a Type `<select>` change right after the key fill can drop the
  key. Verify `StagingSchemaJson` before Publish.
- Drive UI in **small batches (≤6 stateful steps), verify each**. Activity-type `<select>` option
  values are **numeric ids**, not keys.
- Save draft on `/activities/new` keeps URL at `/activities/new`; reach the activity via
  `/activities/{id}` or My Activities to submit.
- Keep psql calls SOLO (a non-zero exit cancels sibling tool calls in the same message). Any
  failing tool call cancels its batch.

## ⏯ RESUME HERE (for next runner — Sonnet recommended for the grind)

**Why a fix is needed:** the Mini-CEX *form schema* (12 fields/3 sections, scale bindings) built
and saved correctly, BUT the Workflow + Credit JSON I pasted used the WRONG format
(`"action"` not `"key"`; no `version`/`initial_state`; flat `epa_field` instead of nested
`curriculum_item_match`). Ground-truth DB confirms: `ActivityTypes[11]` is still **Version 1**,
`CreditRulesJson = {"counts_for":[]}`, `WorkflowJson` = my bad draft. So publish did NOT take a
valid workflow/credit. Two Dlamini Mini-CEX drafts (activities 1 & 2) exist but can't progress
until the workflow is fixed. Browser got signed out.

### Step A — re-login + fix AT 11 workflow/credit, then republish
1. Browser: http://localhost:5080 → sign in **Mbatha** `mbatha@kgk.wombat.local` / `Mbatha@KGK2026!`.
2. Go to `/admin/activity-types/11` → **Workflow** tab → clear the textarea → paste EXACTLY:
   ```json
   {"version":1,"initial_state":"draft","states":[{"key":"draft","label":"Draft"},{"key":"submitted","label":"Submitted"},{"key":"rated","label":"Rated"},{"key":"completed","label":"Completed","terminal":true}],"transitions":[{"key":"submit","from":"draft","to":"submitted","actor":"role:Trainee"},{"key":"accept","from":"submitted","to":"rated","actor":"field:assessor_user_id"},{"key":"complete","from":"rated","to":"completed","actor":"field:assessor_user_id"},{"key":"recall","from":"submitted","to":"draft","actor":"role:Trainee"}]}
   ```
3. **Credit** tab → clear → paste EXACTLY:
   ```json
   {"counts_for":[{"curriculum_item_match":{"epa_field":"epa_id"},"minimum_level_field":"overall_level","amount":1}]}
   ```
4. **Save draft** → verify in DB (solo psql) that staging took:
   `SELECT "StagingWorkflowJson","StagingCreditRulesJson" FROM "ActivityTypes" WHERE "Id"=11;`
   Both must be non-empty and match. THEN click **Publish** → expect "Published version 2".
   Verify: `SELECT "Version","CreditRulesJson"::text FROM "ActivityTypes" WHERE "Id"=11;` → Version 2,
   credit shows the nested curriculum_item_match rule.

> **This is the canonical workflow/credit JSON for ALL the 4-state assessor types** (mini_cex,
> cbd, acat, dops). Reuse it verbatim (only the per-type form fields differ). Procedure_log uses a
> 2-state `draft→logged` workflow; MSF uses `draft→open→closing→closed`. Build form schema via the
> visual builder; paste workflow/credit JSON via the textareas. **Format gotcha that bit me:**
> transitions use `"key"` (the action name), `from`/`to`, `actor`; credit nests the EPA match under
> `curriculum_item_match`. See `scenario-paediatrics.md` lines 293-338 for the parser DSL reference.

### Step B — re-verify the two existing Dlamini drafts still match v2
Activities 1 (level-3 overall) and 2 (level-4 overall) were created against v2 form. After republish
they should still submit fine. Reach each via `/activities/{id}`, Submit (actor role:Trainee).

## Progress
- [x] Restore after-act-2-replay (clean), start dev server, gather facts.
- [x] 3.A pre: mini_cex_paed v2 built + published (12 fields/3 sect, correct workflow/credit). Snapshot `act3R-minicex-published`.
- [x] 3.A/3.B/3.C — Dlamini ×2 Mini-CEX (lvl 3+4), Naidoo accept+complete. PAED-001: 2/30 reached 1. Snapshot `act3R-A-C`.
      Then: Dlamini ×2 Mini-CEX already drafted (act 1 = overall 3, act 2 = overall 4); Submit both →
      Naidoo (`naidoo@kgk…`/`Act2Pass!123`) accept+complete both → verify PAED-001 item 2 credit
      (expect CountsSoFar=2, MinimumLevelReachedCount=1; Dlamini stage 3, min 4 → only the lvl-4 reaches)
      + `/portfolio/progress` shows it.
- [x] 3.D — build procedure_log_paed (id 15) v2: form = procedure_code(Choice), supervision_level(Scale→scale2),
      self_rating(Scale); workflow `{"version":1,"initial_state":"draft","states":[{"key":"draft","label":"Draft"},{"key":"logged","label":"Logged","terminal":true}],"transitions":[{"key":"log","from":"draft","to":"logged","actor":"role:Trainee"}]}`;
      credit `{"counts_for":[{"curriculum_item_match":{"epa_field":"epa_id"},"minimum_level_field":"supervision_level","amount":1}]}` (add an `epa_id` EPA field too).
      du Plessis (`duplessis@kgk…`/`Act2Pass!123`) ×5 logs on PAED-011 (supervision 2,2,3,3,4) → expect PAED-011 item 12: 5/30, reached 3 (stage-2 min 3).
- [x] 3.E — build dops_paed (id 14) v2 (4-state workflow/credit verbatim from Step A; minimum_level_field=overall_level):
      form = epa_id(EPA), assessor_user_id(User), procedure_code(Choice incl "Lumbar puncture (infant)"),
      indication(Text), complications(Text), 5 Scale steps (preparation/consent/landmarks/technique/aftercare→scale2)
      + overall_level(Scale→scale2). **Before play:** NULL Patel training: `UPDATE "AssessorProfiles" SET "TrainingCompletedOn"=NULL WHERE "UserId"='6b236065-634d-4309-9115-c9eb95b15bbd';` (document the tweak; record old value 2021-05-10 to restore if wanted).
      Mahlangu (`mahlangu@kgk…`/`Act2Pass!123`) DOPS all steps + overall = level 2 → Patel accept+complete.
      **Observe:** does Wombat block/flag an in-training (null TrainingCompletedOn) assessor completing? And
      PAED-010 item 11 credit for a STAGE-1 trainee where stage-min JSON has no "1" key {"2":2,"3":3,"4":4}
      (what does GetMinimumLevelForStage fall back to? expect volume 1; reached = depends on fallback).
- [x] 3.F — build msf_paed (id 16) v2: workflow draft→open→closing→closed (open actor role:Trainee; closing/closed actor `creator`).
      Molefe (`molefe@kgk…`/`Act2Pass!123`) open MSF; advance via /admin/jobs MsfClosing or workflow widget to closed.
- [x] 3.G — Mahlangu stale Mini-CEX (activity 10, backdated 15d). Smit panel shows "No stalled requests" — gap F-3G-1.
- [x] 3.H — Audit log clean: all lifecycle events present, [PRINCIPAL] intact, no JsonException.
- [x] 3.I — Dashboard sweep: all 5 trainees verified. No crashes. See findings in current_state.md.
- [x] Snapshots: act3R-A-C, act3R-D, act3R-E, act3R-F, act3R-final — all taken.

## Findings log (fill as I go)
- F-REBUILD-1: all `act3-*` snapshots corrupted by another project (user: ignore; rebuilt from after-act-2-replay).
- F-REBUILD-2 (mine, process): builder Form schema persists fine, but the Workflow/Credit textareas need the
  exact parser JSON (key/from/to/actor; nested curriculum_item_match) — easy to get wrong; verify staging before Publish.
