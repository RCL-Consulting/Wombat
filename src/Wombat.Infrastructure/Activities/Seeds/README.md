# Wombat starter activity seeds

These JSON trios are the first-run starter kit for the activity platform. Each folder contains:

- `schema.json` for the form definition
- `workflow.json` for the lifecycle and actor rules
- `credit.json` for curriculum-credit behavior

Seeded types:

- `mini_cex`: Generic mini clinical evaluation exercise mapped to an EPA and scored on the seeded `or_scale`.
- `dops`: Generic direct observation of procedural skills. Uses the seeded `procedure_catalogue` for the procedure picker and also captures an EPA for current curriculum crediting.
- `cbd`: Generic case-based discussion mapped to an EPA and scored on the seeded `or_scale`.
- `acat`: Generic acute-care assessment mapped to an EPA and scored on the seeded `or_scale`.
- `reflective_note`: Structured reflective note using the situation-task-action-result frame, with speciality-admin approval and no direct credit. (The STAR acronym is reserved for the formal Statement of Awarded Responsibility artefact — see the `EntrustmentDecision` aggregate.)
- `procedure_log`: Self-logged procedure record using the seeded `procedure_catalogue`.
- `research_output`: Publication/poster/presentation capture with speciality-admin verification.
- `teaching_session`: Teaching log with basic acceptance workflow.
- `qi_project`: Quality-improvement project using three fixed PDSA sections in v1 instead of repeatable arrays.
- `journal_club`: Simple logged journal-club attendance record.

Caveats:

- The workflow grammar currently supports `field:<field_key>` actor rules, so the WBA seeds target the named assessor in `assessor_user_id`.
- `procedure_log`, `research_output`, `teaching_session`, `qi_project`, and `journal_club` currently seed with no credit directives because the curriculum model only supports EPA-targeted progress today.
- `dops` captures both a procedure and an EPA. The procedure catalogue is the operational record; the EPA field keeps the seed immediately useful with the current curriculum-credit engine.
- `procedure_catalogue` is a reference table, not embedded in schema JSON. Choice fields can reference it via `"catalogue": "procedure_catalogue"`.
