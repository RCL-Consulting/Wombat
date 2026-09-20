# T020 — Seed initial activity types

**Phase:** 3 — Seeded activity types
**Depends on:** T019
**Blocks:** T011, T016

## Goal

Ship Wombat with a starter set of activity types so a fresh install is immediately useful. These seeds also serve as worked examples admins can copy and adapt. Every seed is data — created by the same mechanism an admin would use through the builder.

## What to seed

Each seed is a JSON trio (schema, workflow, credit rules) committed under `src/Wombat.Infrastructure/Activities/Seeds/` and loaded by `DataSeeder` if it doesn't already exist.

### Work-based assessments (the classic four)

1. **`mini_cex`** — Mini Clinical Evaluation Exercise.
   - Schema sections: Context (setting, presenting complaint, complexity), Assessment (history, exam, reasoning, communication, professionalism, overall — each a scale against `or_scale`), Feedback (strengths, improvements, plan).
   - Workflow: `requested → accepted → completed`, with `declined` and `cancelled` branches. Actor rules: subject (trainee) requests, named assessor (via `user` field `assessor_user_id`) accepts/declines/completes, either party can cancel.
   - Credit: one toward the matching CurriculumItem (EPA from an `epa_id` field) when `overall >= 4`.
2. **`dops`** — Direct Observation of Procedural Skills.
   - Schema: Procedure (pick from a procedure list — this is a separate choice field seeded from a procedure catalogue), Indications, Complexity, Assessment (six criteria), Feedback.
   - Workflow: same as Mini-CEX.
   - Credit: one toward the matching procedure requirement (ties into T020's logbook piece below).
3. **`cbd`** — Case-based Discussion.
   - Schema: Case summary, Medical record review, Clinical reasoning, Management plan, Medical ethics, Feedback.
   - Workflow: same as Mini-CEX.
   - Credit: same shape as Mini-CEX.
4. **`acat`** — Acute Care Assessment Tool.
   - Schema: Acute presentation, Initial assessment, Investigation, Management, Handover, Team leadership, Feedback.
   - Workflow: same as Mini-CEX.
   - Credit: same shape.

### Reflection

5. **`star_reflection`** — Situation-Task-Action-Result reflection.
   - Schema: four longtext sections (Situation, Task, Action, Result), plus an EPA picker and an optional free-text "what I learned".
   - Workflow: `draft → submitted → approved/declined`. Subject (trainee) drafts + submits. SpecialityAdmin in same speciality approves/declines. Decline requires note (feedback). Resubmit: subject may edit after decline, which returns to `draft`; re-submitting moves back to `submitted`.
   - Credit: none directly (reflections support portfolio but don't tick EPA boxes). Curriculum items can still require a minimum count via a separate rule type — defer; seed with no credit for now.

### Procedure logbook

6. **`procedure_log`** — a trainee's log of a procedure they performed.
   - Schema: Procedure (choice), Date (`date` field type), Patient age band (choice, no PII), Indication (longtext), Role (choice: `primary`, `assistant`, `observer`), Supervision (choice: `independent`, `indirect`, `direct`), Complication (choice: `none`, `minor`, `major`, with longtext if not none), Reflection (longtext, optional).
   - Workflow: `logged` (single terminal state). No approval needed; it's the trainee's own record. Optional later: a `verification_requested → verified` branch if an institution requires supervisor sign-off.
   - Credit: counts toward a "procedure requirement" — this requires a slight extension to the credit rules so a credit directive can target a procedure catalogue entry rather than only a CurriculumItem. Document the extension; implement in T018 as a follow-up if necessary.

### Scholarly activity

7. **`research_output`** — publication, poster, or presentation.
   - Schema: Type (choice: journal article, conference poster, oral presentation, book chapter), Title, Authors, Venue, Date, Trainee's role (choice: first author, co-author, senior author, presenter), Abstract (longtext), URL/DOI, File upload (PDF, 5MB max).
   - Workflow: `draft → submitted → verified/rejected`. SpecialityAdmin verifies.
   - Credit: counts toward a "scholarly activity requirement" on the curriculum.
8. **`teaching_session`** — a teaching session the trainee delivered.
   - Schema: Topic, Audience (choice + number), Duration, Format (choice), Self-evaluation (longtext), Feedback received (longtext, optional).
   - Workflow: `draft → submitted → accepted`.
   - Credit: counts toward teaching requirement.
9. **`qi_project`** — quality improvement project.
   - Schema: Project title, Problem statement, Team, Measures, PDSA cycles (repeatable section — may need a schema extension for arrays; defer array support to a later task if not in T017's DSL), Outcomes, Reflection.
   - Workflow: `draft → submitted → reviewed`.
   - Credit: counts toward QI requirement.
10. **`journal_club`** — journal club attendance.
    - Schema: Date, Article reference, Trainee's role (presenter/attendee), Key learning points (longtext).
    - Workflow: `logged` single terminal.
    - Credit: counts toward journal club requirement.

## What to do

1. For each activity type, write the three JSON files under `src/Wombat.Infrastructure/Activities/Seeds/{key}/{schema.json,workflow.json,credit.json}`.
2. Extend `DataSeeder` to load each seed: if an `ActivityType` with the given `Key` does not exist, create it at version 1. Do **not** overwrite existing types — admins may have edited them.
3. Seed the `or_scale` (O-R Scale, levels 1–5) if T014 didn't already.
4. Seed a procedure catalogue (`ProcedureCatalogueEntry` — Id, Key, Name, Category) with ~20 common procedures for the demo speciality. This feeds the `choice` field in `dops` and `procedure_log`. Extend the schema DSL's `choice` type to accept a `catalogue` reference in addition to a literal option list, if not already supported (flag this as a potential schema DSL extension).
5. Document each seed in `src/Wombat.Infrastructure/Activities/Seeds/README.md` — what it is, what real-world instrument it represents, caveats.
6. Unit test: every seed JSON parses cleanly through `FormSchemaParser`, `WorkflowParser`, `CreditRulesParser`. This test runs in CI and guards against seed drift.

## Files created

- `src/Wombat.Infrastructure/Activities/Seeds/{mini_cex,dops,cbd,acat,star_reflection,procedure_log,research_output,teaching_session,qi_project,journal_club}/{schema,workflow,credit}.json`
- `src/Wombat.Infrastructure/Activities/Seeds/README.md`
- Extensions to `DataSeeder` to load them.
- New reference table `ProcedureCatalogueEntry` with a migration.
- `tests/Wombat.Infrastructure.Tests/Activities/SeedParseTests.cs`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — seed parse tests green.
- [ ] Fresh install: `dotnet Wombat.Web.dll --seed` creates all ten activity types visible in the admin `ActivityTypesList` page.
- [ ] Manual: as a trainee, log a Mini-CEX and a Procedure Log against the demo curriculum. Confirm curriculum progress increments.
- [ ] As an admin, open the `mini_cex` type in the builder. Confirm every section renders. Make a harmless edit (change help text), publish version 2. Confirm existing Mini-CEX activities still render on version 1.

## Notes & gotchas

- These seeds define the Wombat "starter kit" users first encounter. Invest in making them pedagogically sound. A SpecialityAdmin can always clone and adapt per programme.
- The procedure catalogue is a separate reference table — not jsonb. `choice` fields can either list options inline or reference a catalogue. The catalogue gives a stable set that evolves outside the form schema.
- A **schema DSL extension** is required for repeatable sections (QI PDSA cycles). Decide in this task whether to extend T017's DSL now or defer QI to a later seed. Recommended: extend, because arrays are broadly useful and QI is a common requirement.
- Copyright on the real instruments: Mini-CEX, DOPS, CbD and ACAT are broadly available in published academic literature and the *structure* is not copyrighted. The exact wording of the criteria on commercial forms may be. Use plain, generic language in the seed schemas; do not copy text from a specific institution's form.
- After this task, the platform proves itself: any future activity type is a data edit, not code.
