# T050 — Scenario doc corrections (absorb Act 1 audit findings)

The Playwright route-and-surface audit (commit `c07b71a`) found three hard gaps, three route mismatches, and four UX-friction items across Act 1 of `Rewrite/scenario-paediatrics.md`. This task absorbs every doc-side finding so the scenario plays as-written against the current dev build. Code-side findings ship as T051–T055.

## Changes

### Phase / step reshuffling

Old Phase 1.A (Provision Prof Mbatha first) is impossible: the invitation form needs an institution before it will accept the invitation. New ordering:

- **Phase 1.A — Institutional structure** (was 1.B): bootstrap admin logs in, creates institution, speciality, sub-speciality. Steps 1.1–1.4.
- **Phase 1.B — Provision Prof Mbatha** (was 1.A): bootstrap admin issues `InstitutionalAdmin` invitation scoped to KGK; Prof Mbatha accepts. Steps 1.5–1.6.
- Phases 1.C through 1.F keep their names; step numbers unchanged (1.7 onward).

Prof Mbatha demotes from global `Administrator` to `InstitutionalAdmin` because the invitation surface does not expose the `Administrator` role. The "Note on Administrator scope" rationale block carries the change.

### Step-level edits

| Step | What changes |
|---|---|
| 1.1 | Drop "Viewing as Administrator" expectation; describe actual dashboard header. |
| 1.2 | Was old 1.4 (institution). |
| 1.3 | Was old 1.5 (speciality). Link label is `Specialities`, not `Manage specialities`. |
| 1.4 | Was old 1.6 (sub-speciality). |
| 1.5 | Was old 1.2. Route → `/admin/invitations` (form embedded in list). Role → `InstitutionalAdmin`. Institution → KGK. Remove First/Last name fields (not on form today — T051 adds them). Submit label → `Issue invitation`. |
| 1.6 | Was old 1.3. Acknowledge that flow is unverified until T051 lands; describe the accept-invitation page generically. |
| 1.7 | Replace prescription with workaround prose: reuse the seeded `EntrustmentScale` (`DataSeeder.cs:113-133`). Mark capability gap as tracked by T054. |
| 1.8 | Acknowledge the extra `Required knowledge and skills` field. |
| 1.11.a | Note the `Scope Id` raw-spinbutton UX friction; T053 will replace with a picker. |
| 1.11.b | `LongText` → `Long text`. |
| 1.11.c | Replace JSON with corrected schema (parser-accepted shape). Add a callout describing the actor DSL. |
| 1.11.e | Clarify Publish renders next to Save draft only after an unsaved draft exists; folds T055 into doc-only. |
| 1.12 | Add a one-line callout that all 9 summarised types use the same actor DSL as Step 1.11.c. |
| 1.13 | Column wording: `Published` / `Draft` / `Status` as three separate columns. |

### Findings-summary section

Replace the placeholder findings-summary with a pointer to commit `c07b71a` and the `scenario-act1-fixes-plan.md` triage. The Actual/Gap blocks in each step revert to blank per scenario convention; the historical audit findings live in git and in the plan.

### Cast / phase preamble

- Update "Cast" table to show Prof Mbatha as `InstitutionalAdmin` (scoped to KGK), not global `Administrator`.
- Update Phase 1.A preamble: "Prof Nolwazi Mbatha, alone, after hours" → bootstrap admin opens the setup; Mbatha joins after Phase 1.B.

## Out of scope

- T051–T055 (code-side fixes): tracked separately in `scenario-act1-fixes-plan.md`.
- Acts 2–5: still undrafted; depend on Act 1 playing cleanly first.

## Verification

- `dotnet build Wombat.sln -c Release` — must remain clean (this task changes only Markdown).
- Tests unaffected.
- Spot-read the rewritten doc end-to-end and confirm step ordering and cross-references are consistent.

## Definition of done

- `Rewrite/scenario-paediatrics.md` plays as-written for steps 1.1–1.13, modulo the two flagged dependencies (T054 for 1.7 scale, T051 for 1.5 name capture).
- All Actual/Gap lines blank per scenario convention.
- `Rewrite/current_state.md` records the commit hash and recommends model for the next active task.

## Files touched

- `Rewrite/scenario-paediatrics.md`
- `Rewrite/Tasks/T050-scenario-doc-corrections.md` (this file)
- `Rewrite/current_state.md`
