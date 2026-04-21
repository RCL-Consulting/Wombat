# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**Block 3 complete.** Next up: **T035 — Assessor training status field** (Block 4, first task). Model: Sonnet.

Adds `AssessorProfile.TrainingCompletedOn` (`DateOnly?`) and surfaces it on the admin assessor list. No blocking behaviour — just visible record. Scope in `Rewrite/practical-plan.md` §T035. ½ day.

## This session at a glance

Block 3 closed. T034 shipped on `master`:

- **T034 — EPA core/elective + stage-indexed supervision levels** — `dd18b66`

The curriculum now understands two EPA-programme-design primitives the plan called for: EPAs are tagged `Core` or `Elective`, and curriculum items can specify a per-training-year supervision level that overrides the flat minimum. The trainee dashboard shows the effective minimum for the trainee's current year so they know what level to hit.

## Last completed

**T034 — EPA core/elective flag + stage-indexed supervision levels.**

- New `EpaCategory` enum (`Core=0`, `Elective=1`) plus `Epa.Category` property (defaults `Core` via column default on migration). `CreateEpaCommand` takes an optional `Category` parameter (defaults `Core` — non-breaking); `UpdateEpaCommand` takes it positionally. `EpaDto` surfaces it to the admin UI.
- `CurriculumItem.MinimumLevelByStageJson` optional string column carrying a JSON object keyed by training year (e.g. `{"1":2,"2":3,"3":4}`). Lookup helper `GetMinimumLevelForStage(int?)` returns the override for the stage if present, otherwise the flat `MinimumLevelOrder`. `ParseStageOverrides` drops entries with non-integer keys, keys ≤ 0, levels outside 1–20, or non-integer/non-string values. `NormalizeStageOverridesJson` canonicalises to ordered keys and returns `null` for empty input so `{}` doesn't round-trip.
- `AddCurriculumItemCommand` and `UpdateCurriculumItemCommand` take an optional `MinimumLevelByStageJson`; validators reject malformed JSON or objects whose entries all drop during parse. Normalised before persist.
- Migration `20260421170000_EpaCategoryAndStageLevels` — hand-written with Designer.cs and matching `ApplicationDbContextModelSnapshot` updates. Up: `Epas.Category` (`integer NOT NULL` default 0) + `CurriculumItems.MinimumLevelByStageJson` (`text` nullable). Down: drops both.
- Admin UI: EPA form has a Category dropdown; curriculum items table has a "Per-stage minima" column with an inline JSON editor (textbox with placeholder `{"1":2,"2":3}`). The add form includes a full-width labelled field with guidance text.
- Trainee dashboard's "Curriculum progress" card now shows `Minimum level N (year X) · reached M / R` beneath each progress bar, using the trainee's current year computed from `ProgrammeStartDate`. `GetTraineeDashboardSummaryQueryHandler.ComputeTraineeStage(start, today)` is public for test coverage and returns `null` for programmes that have not yet started.
- 7 Domain tests for `CurriculumItem` helpers (stage override lookup, invalid-entry parse behaviour, normaliser ordering, empty-object returns null). 5 Application tests for handlers (EPA Category default, create-with-Elective, update persists, CurriculumItem stage overrides round-trip, validator rejects bad JSON). 7 Application tests for the dashboard stage override (year-3 override applied, year-6 falls back to flat, stage theory for day-0/day-364/day-365/day-730, null when programme starts in future).

## Plan this session works against

`Rewrite/practical-plan.md` — Block 3 done. Block 4 (T035, T036) is the final block.

## Block 4 sequence

1. T035 — Assessor training status field (active)
2. T036 — Accreditor-specific export template

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass (7 new for stage-override helpers)
- Application tests — 165/165 pass (12 new: 5 EPA/curriculum handlers + 7 dashboard stage override)
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T034 compromises

- **JSON-textbox admin editor, not a per-row form.** The stage-override editor is a single text input that expects a JSON object. A row-per-year editor would be friendlier but doubles the UI complexity for an admin-only tool. The validator covers malformed input; bad entries reject with a clear message.
- **Stage override does not affect CreditApplier.** Credit-at-minimum-level decisions still compare against `CurriculumItem.MinimumLevelOrder` when an activity completes — which is the right behaviour for the stage at activity time (the flat value is what the programme considered baseline). Changing this to time-travel through stage overrides on retroactive credit was intentionally out of scope. The dashboard reads the effective minimum live so the trainee and committee see the right target today.
- **Year boundaries are simple day-math.** `ComputeTraineeStage` uses `daysElapsed / 365` — no leap-year correction, no academic-year calendar. Close enough for a UI hint. Programmes that need exact academic-year boundaries can migrate to a stage-field on `TraineeProfile` later.
- **No data migration for existing EPAs.** Every EPA lands on `Category = Core` because the column default is 0. Admins adjust via the EPA edit form if needed.

## What remains (operational, not code — carried forward from T016)

- Execute `deploy/README.md` first-boot checklist against a real Linode server
- Configure DNS, TLS certificate (Caddy auto-provisions via ACME)
- Set production secrets in `/opt/wombat/config/wombat.env`
- Run `--seed` to provision the admin user and seeded activity types
- Revoke UPDATE/DELETE on AuditEntries table after first migration

## Companion reference docs

- `EPA Book/evaluation.md` — 92-requirement book scorecard (reference, not todo list)
- `EPA Book/critique.md` — literature-backed reasoning for practical-plan compromises
- `Rewrite/book-fidelity-plan.md` — superseded; kept only because `critique.md` cites it

## Last verified commits

- `dd18b66` — T034 (EPA core/elective + stage-indexed supervision levels)
- `9910ba1` — T033 (per-trainee per-EPA trajectory chart)
- `2e02a1e` — T032 (sampling-concentration warning on review detail)
- `ac4fdb9` — T031 (formative-only committee review mode)
- `c9b00d0` — docs: record T030 commit hash
- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
