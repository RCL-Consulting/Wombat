# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T037 — NavMenu icon hotfix (GUI review cluster 1).** Model: Sonnet.

First task of the GUI review plan. `NavMenu.razor` has 31 `<span class="bi bi-*">` references that render nothing (Bootstrap Icons font is not loaded per CLAUDE.md). Replace with `<Icon Name="..." />` + Lucide SVGs; add any missing icons to `src/Wombat.Web/wwwroot/icons/`; verify in a browser for each authenticated role. Scope in `Rewrite/gui-review-plan.md` §T037. ½ day.

## This session at a glance

Practical plan closed. **T036 (accreditor-specific export template) deferred indefinitely** — WBA is new locally, no accreditor format spec is forthcoming, and a speculative generic template would likely be rewritten when a real spec lands. T023's portfolio PDF covers the trainee-facing export in the meantime.

New plan drafted: `Rewrite/gui-review-plan.md`. Design-system audit across ~65 pages + 15 shared components, split into six clusters (T037–T042). Rubric + per-cluster page list + suggested sequencing inside. ~8 working days total. T037 (NavMenu icon hotfix) runs first because it's cheap and makes browser verification on every other cluster meaningful.

## Last completed

**T035 — Assessor training status field.**

- `AssessorProfile.TrainingCompletedOn` — nullable `DateOnly`. No behaviour change.
- Hand-written migration `20260421180000_AssessorTrainingStatus` (+ Designer.cs + `ApplicationDbContextModelSnapshot` updates) adds `AssessorProfiles.TrainingCompletedOn date NULL`.
- `AssessorProfileDto`, `CreateOrUpdateAssessorProfileCommand`, `GetAssessorProfileByIdQuery`, and `ListAssessorsForSpecialityQuery` carry the new field. Command parameter defaults to `null` so existing callers stay non-breaking.
- `AssessorsList.razor` gained a "Training completed" column rendering the date in `yyyy-MM-dd` or `Not recorded`. `AssessorProfileEdit.razor` has a date picker with a short guidance blurb. Form model round-trips the value.
- 3 Application tests cover the round-trip: create/update persists the date, get-by-id surfaces it, and a list query returns the training status for a mixed-population (recorded + blank) set.
- No enforcement anywhere; the field is visibility-only, consistent with the §T035 "answers 'do your assessors have training?' with a list" intent.

## Plan this session works against

`Rewrite/gui-review-plan.md` — design-system audit of ~65 pages + 15 shared components. Cluster 1 (T037) active.

`Rewrite/practical-plan.md` — closed: T035 done, T036 deferred indefinitely.

## GUI review sequence

1. T037 — NavMenu icon hotfix (active — ½ day)
2. T038 — Trainee surface (1.5 d)
3. T039 — Committee flow (1.5 d)
4. T040 — Admin hierarchy (2 d)
5. T041 — Activity platform (2 d)
6. T042 — Account & auth shell (1 d)

## Block 4 / practical-plan sequence (closed)

1. ✅ T035 — Assessor training status field
2. 🚫 T036 — Accreditor-specific export template (deferred — WBA new locally, no accreditor spec)

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 45/45 pass
- Application tests — 168/168 pass (3 new for training status round-trip)
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T035 compromises

- **No validation that the date is plausible.** The date picker accepts any `DateOnly?` — a far-future date would be stored as-is. The field is information-only and admin-only, so the risk is cosmetic; add a sanity guard if a programme starts filling in future placeholder dates.
- **No browser-level verification this session.** List/edit wiring is a direct column-through, identical in shape to existing profile fields; trusted from the Application-layer round-trip tests.

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

- `9d60cd7` — T035 (assessor training status field)
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
