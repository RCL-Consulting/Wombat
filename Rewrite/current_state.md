# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**Block 2 complete.** Next up: **T033 — Per-trainee per-EPA trajectory chart** (Block 3, first task). Model: Sonnet.

Server-side SVG trajectory line chart (one dot per observation, rating on Y, date on X), rendered on the trainee EPA detail view and `ReviewDetail.razor`. No JS dep, no cohort comparison. Scope in `Rewrite/practical-plan.md` §T033. 2 days.

## This session at a glance

Block 2 closed. T032 shipped on `master`:

- **T032 — Sampling-concentration warning on review detail** — `2e02a1e`

Hospital now has: defensible entrustment records end-to-end (Block 1), plus formative review mode (T031) and per-EPA sampling warnings that surface assessor concentration, single-source bias, and thin assessor pools before the committee records a decision.

## Last completed

**T032 — Sampling-concentration warning on review detail.**

- New MediatR query `GetSamplingConcentrationWarningsQuery(ReviewId)` in `Wombat.Application.Features.CommitteeDecisions`. Returns a `SamplingConcentrationReportDto` with per-EPA flags (`OneAssessorOverHalf`, `SingleSource`, `FewerThanThreeAssessors`) plus review-level totals. Handler parses `DataJson` client-side after pulling activities by trainee+period, so it works under both PostgreSQL and the in-memory provider used in tests.
- Source classification maps the four rated seed keys to the book's four-sources taxonomy: `mini_cex`/`dops` → DirectObservation, `cbd`/`acat` → Conversation. Only those four keys are treated as "rated" (they carry `epa_id` + `assessor_user_id` in their schema). Product/longitudinal seeds are ignored for this check.
- `ReviewDetail.razor` loads the report alongside the review and renders a dismissible `<Alert Kind="warning">` above the details grid when any EPA has a warning flag. Reuses the existing `Alert.Dismissible` affordance — no new CSS, no new component.
- 7 Application tests: all-clear path, no-data path, each of the three flags in isolation, period-window filter, unrated-activity-type exclusion.
- No new aggregate, no projection table, no migration. Single query, single component — matches the §T032 scope verbatim.

## Plan this session works against

`Rewrite/practical-plan.md` — Block 2 complete; Block 3 (T033, T034) is next.

## Block 3 sequence

1. T033 — per-trainee per-EPA trajectory chart (active)
2. T034 — EPA core/elective + stage-indexed supervision levels

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 38/38 pass
- Application tests — 146/146 pass (7 new for sampling concentration)
- Architecture tests — 19/19 pass
- Web tests — 33/33 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T032 compromises

- **MSF is not included in per-EPA sampling stats.** MSF is a cross-cutting longitudinal-observation instrument and is not currently linked to a specific EPA. A committee reading the warnings gets the assessor/source breakdown from direct-observation and conversation instruments only. If MSF respondents should count toward distinct-assessor/source counts for each EPA later, the query can be extended without changing the DTO shape.
- **Single-source warning can only trigger for DirectObservation-only or Conversation-only evidence.** With only four rated seed types across two sources, flagging "all ratings from one source" captures the realistic book-sense warning. Once more source-diverse seeds arrive (product-eval instruments with assessor + epa), the `SourceByActivityKey` map in `GetSamplingConcentrationWarnings.cs` is the single point to extend.
- **One-assessor-over-half is suppressed for single-rating EPAs.** A one-rating EPA is always "100% from one assessor" — that information is redundant with the fewer-than-three-assessors warning, so the flag requires ≥2 ratings to fire.
- **Dismiss state is session-local.** The `Alert` component's built-in dismiss sets a component-level bool; no server persistence. Re-opening the review re-shows the warning. Intentional: committees should see it on every visit.

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

- `2e02a1e` — T032 (sampling-concentration warning on review detail)
- `ac4fdb9` — T031 (formative-only committee review mode)
- `c9b00d0` — docs: record T030 commit hash
- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
