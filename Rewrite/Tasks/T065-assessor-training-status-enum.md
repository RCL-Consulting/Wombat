# T065 — Assessor training status: clarify enum vs date

> **✅ SHIPPED 2026-05-30 (Opus) — Option B chosen** (commit `2562c2a`). Added the
> `AssessorTrainingStatus` enum (NotStarted/InTraining/Provisional/Trained) to `AssessorProfile`
> alongside the existing `TrainingCompletedOn` date. Migration
> `20260530104328_AssessorTrainingStatusEnum` (dotnet-ef scaffolded) with a backfill (recorded
> completion date → Trained, else NotStarted), verified applied to the dev DB. DTO + command + 3
> handlers + edit-page picker (date gated to Provisional/Trained) + list column; browser round-trip
> verified. Tests: existing date test updated to pass Trained, +1 handler test (date cleared for
> non-completed statuses). Closes Act-3 finding **F-3E-1** (in-training assessor now representable).
> **Note:** surfaces status (flagging); hard-*blocking* an in-training assessor from completing is a
> separate product decision, deliberately left out.

---

The `Rewrite/scenario-paediatrics.md` Act 2 cast called for a `TrainingStatus` enum on `AssessorProfile` with three values: `Trained` / `In training` / `Provisional`. The 2026-05-27 Act 2 play-through (Finding **A2-5**) discovered T035 actually shipped a single `TrainingCompletedOn` date column — "Date the assessor completed assessor training. Leave blank if unrecorded." The shapes are not equivalent: a date can express "not recorded" but cannot distinguish "actively in training" from "provisional (profiled but no training yet)".

Decision required: **revise the scenario** to refer to the date only, OR **extend T035** with the enum and keep the date as a sub-field.

## Two options

### Option A — Doc fix only (cheaper)

Update `Rewrite/scenario-paediatrics.md` Step 2.4 to refer to the `Assessor training completed` date with explicit dates per assessor:
- Trained assessors: a past date (e.g. Zulu 2018-12-01, Naidoo 2020-03-15, Botha 2019-08-20, Khumalo 2021-05-10).
- In-training assessor (Patel): blank.
- "Provisional" semantic dropped — the scenario was overspecified.

No code changes. Effort: ~15 minutes.

### Option B — Add the enum (more faithful to the original CMSA / faculty-dev concept)

Add `TrainingStatus` enum to `Wombat.Domain.Assessors.AssessorProfile`:
- `NotStarted` (default; equivalent to blank date today)
- `InTraining`
- `Provisional` (training done but pending sign-off / certification)
- `Trained` (fully certified)

Persist as a new column; hand-written migration + Designer + snapshot (per CLAUDE.md migration convention). UI: replace the existing date input with a `<select>` for status + an optional date that's only visible/enabled when status is `Provisional` or `Trained`. Reporting / dashboards could surface "X provisional assessors awaiting sign-off" as a Coordinator-dashboard KPI later.

Effort: ~1.5–2 hours including migration + UI + tests.

## Recommendation

**Option A** — the date column is sufficient for the assessor lifecycle in practice. "Provisional" was scenario shorthand for "training-completed but in a probation window" — equivalent to "training-completed date <12 months ago". A computed display can derive that from the date if needed.

If a stakeholder later asks for the explicit "Provisional" state (e.g. for an accreditation report), revisit with Option B.

## If Option A is chosen

`Rewrite/scenario-paediatrics.md` Step 2.4 update only. Replace the `Training status (T035 field)` column in the cast table with `Training completed (date)`:

| User | Training completed | Note |
|---|---|---|
| Dr Thandi Zulu | 2018-12-01 | Senior assessor — panel chair in Phase 2.G. |
| Dr David Naidoo | 2020-03-15 | |
| Dr Sarah Botha | 2019-08-20 | |
| Dr Mohammed Patel | (blank) | In training; faculty-development sign-off pending. |
| Dr Fatima Khumalo | 2021-05-10 | |

Remove the inline `> Note on training status:` block. Mark A2-5 closed in the Act 2 findings summary.

## If Option B is chosen

Sketch of files touched:
- `src/Wombat.Domain/Assessors/AssessorProfile.cs` — new `TrainingStatus` enum + property.
- `src/Wombat.Infrastructure/Persistence/Migrations/2026XXXXXX_AddAssessorTrainingStatus.cs` + `.Designer.cs` + snapshot update.
- `src/Wombat.Application/Features/Assessors/*` — surface the new field in DTOs + commands.
- `src/Wombat.Web/Components/Pages/Admin/Assessors/AssessorProfileEdit.razor` — picker.
- Tests + scenario doc + handoff.

## Definition of done

- (Option A) Scenario doc updated; Act 2 A2-5 marked closed.
- (Option B) Build clean, migration runs against existing dev DB without data loss, all tests pass, browser-verified picker round-trip, scenario doc updated.

## Files touched

(Option A) Only `Rewrite/scenario-paediatrics.md` + `Rewrite/Tasks/T065-assessor-training-status-enum.md` (this file) + `Rewrite/current_state.md`.

## Estimate

~15 minutes for Option A, ~1.5–2 hours for Option B. **Sonnet.**
