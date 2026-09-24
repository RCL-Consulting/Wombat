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

The CPSA paediatric instruments, seeded by `PaediatricCatalogueSeeder` against the Paediatrics
speciality rather than the demo one. The `_cpsa` suffix is not decoration — see that class for why:

- `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa`: the v11.1 workplace-based
  assessment tools. Assessor-completed, rated on the CPSA six-rung ladder, crediting one encounter
  against the EPA named in `epa_id`.
- `cca_cpsa`, `rca_cpsa`, `chart_stimulated_recall_cpsa` (T120): the same rated shape, with a request
  section drawn from page 8's definitions.
- `reflective_exercise_cpsa` (T120), `clinical_audit_cpsa` and `portfolio_review_cpsa` (T154): **unrated
  evidence that credits nothing** (D6, D7): no `rated_level_field`, `"counts_for": []`. The trainee writes
  each and names a supervisor (`assessor_user_id`, role Assessor), who signs it off or returns it with a
  note. The audit's report is a `text` field holding an `http(s)` link, held to that by a `regex`,
  because Wombat stores no files (D34). The portfolio review is a signed review of a stated period, not a
  re-typed portfolio: the trainee names the period, exports the portfolio for it (Export Portfolio, in the
  menu) and records the export's file name, and the reviewer reads it with the records Wombat does not hold
  (the logbook, teaching, journal club). Its encounter date is the period's last day (`period_to`), so it
  falls in the committee window it is evidence for, as the audit (the day practice was measured) and the
  reflective exercise (the day of the case) are dated by what they are about, not by their sign-off.
- `msf_cpsa`: **system-written, not hand-filed.** One row per EPA a released multi-source feedback
  campaign declared itself evidence for (T121). Its `record` transition and both its sections carry
  `role:Coordinator|role:Administrator`, so a trainee who creates a stray draft from `/activities/new`
  can neither fill it nor complete it. It ships `"counts_for": []` because College decision D8 says
  MSF consumes none of Annexure A's 55 encounters; its value is the evidence link, not a count.
- `learner_feedback_cpsa` (T164, D35): **system-written, the same shape as `msf_cpsa`.** One row per EPA a
  released *learner-feedback* campaign (an MSF template of kind `LearnerFeedback`, answered only by `Learner`
  invitees) declared itself evidence for. Unrated: learners judge the teaching, and nobody states a supervision
  level for it, so it has no `rated_level_field`. It carries `teaching_context_count`, how many distinct
  teaching contexts the returned questionnaires came from, which EPA 15's "two teaching contexts" is about; it
  is reported and gates nothing (§ 3F question 9). A campaign may cover only an EPA whose list names
  `learner_feedback`, which on v11.1 is PAED-015.

## Which instrument a seed is (T122)

A type's instrument identity, `ActivityType.WbaToolKey`, is **not in any file here**. It is the `WbaToolKey` on the
type's `ActivityTypeSeedCatalogue` entry, a key into the College vocabulary that the paediatric catalogue
(`Persistence/Seeds/paediatric-epa-v11.1.json`, `wbaToolVocabulary`) seeds into `WbaTools`. Each EPA's tool list
decides which instruments may be filed against it. The entry's key is required, so a new seed must say which
instrument it is, or say `null`.

- `mini_cex_cpsa` → `mini_cex`, `dops_cpsa` → `dops`, `cbd_cpsa` → `cbd`, `direct_observation_cpsa` →
  `direct_observation`, `msf_cpsa` → `msf`, and each later `<family>_cpsa` seed → `<family>` (`cca`, `rca`,
  `chart_stimulated_recall`, `reflective_exercise`, `clinical_audit`, `portfolio_review`, `learner_feedback`); the generic `mini_cex`,
  `dops` and `cbd` carry the same keys, because they are the same instruments. Everything else is `null`:
  unrestricted (D21).
- The lists bind every instrument, crediting or not. An unrated one (`"counts_for": []`) is judged, and its EPA picker
  narrowed, on its `evidence_epa_field` (T154), so a clinical audit is offered and accepted on PAED-001 to 003 only.
- The seeders write the key when they **create** a type, and the refresher never touches it: it evolves the four
  versioned payloads and nothing else. Existing databases got their keys from the T122 migration. Changing a key
  here reaches an existing database only through a new migration, and until then the seeders log a warning.

Caveats:

- The workflow grammar currently supports `field:<field_key>` actor rules, so the WBA seeds target the named assessor in `assessor_user_id`.
- `procedure_log`, `research_output`, `teaching_session`, `qi_project`, and `journal_club` currently seed with no credit directives because the curriculum model only supports EPA-targeted progress today.
- `dops` captures both a procedure and an EPA. The procedure catalogue is the operational record; the EPA field keeps the seed immediately useful with the current curriculum-credit engine.
- `procedure_catalogue` is a reference table, not embedded in schema JSON. Choice fields can reference it via `"catalogue": "procedure_catalogue"`.
- Every seed that records when its encounter happened names that field at the root of `schema.json`:
  `"observation_date_field": "observed_on"` (T119). `ActivityService` stamps `Activity.ObservedOn` from the
  named field, and every date the product filters, plots, credits and prints comes from that column. A seed
  that carries a date field but forgets the pointer is not refused — it silently falls back to the row's
  audit timestamp, which is the defect T119 exists to remove. `reflective_note` and `qi_project` declare no
  pointer on purpose: a reflection and a months-long QI project have no single encounter date.
- Every seed that produces an entrustment rating names the field carrying it at the root of `schema.json`:
  `"rated_level_field": "overall_level"` (T126). It cannot be inferred — `mini_cex`, `dops` and `acat`
  each declare six `scale` fields and `cbd` five, of which exactly one is the overall judgement. Before
  T126 the only thing that knew was the credit rules' `minimum_level_field`, so a tool crediting nothing
  had no stated rated field at all, and a reader holding an ordinal could not say which ladder it sat on.
  **Nine seeds declare it; the six that rate nothing must not.** `SeedRoundTripTests` asserts exactly
  that correspondence, so a new rated tool that forgets the pointer fails by name. The parser refuses a
  pointer naming a missing field or a field that is not `scale`-typed.

## Editing a seed after it has been seeded (T103)

`DataSeeder` and `PaediatricCatalogueSeeder` only ever *create*. They skip any key that already exists, so
editing a file here used to change nothing on a database where the type was already present, with no error
and no log line.

`ActivityTypeSeedRefresher` runs at startup after both seeders and closes that gap: it canonicalises each
folder through the DSL parsers, compares it against the published version canonicalised the same way, and
publishes a new version when they differ. Three things follow from that:

- **Edit freely, restart, and the change is live for new activities.** Existing activities stay pinned to
  the version they were created against — that is deliberate, and it means a fix here does not reach an
  activity that is already in flight.
- **A type an operator has customised is never reverted.** The refresher skips a type whose newest version
  was published by anyone other than `seed-system`, or which has a draft in flight, and logs why.
- **Adding a folder is not enough.** The key, name, description and display-fields rule live in
  `ActivityTypeSeedCatalogue`; a folder missing from it is never seeded and never refreshed.
  `ActivityTypeSeedRefresherTests.Catalogue_CoversEverySeedFolderOnDisk` fails if the two drift apart.

Set `Wombat__RefreshSeededActivityTypes=false` to turn the republish off. The diff still runs and still
logs what it would have done.
