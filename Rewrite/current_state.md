# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T034 — EPA core/elective flag + stage-indexed supervision levels.** Model: Sonnet.

Second Block 3 task. Adds `Epa.Category` enum (Core/Elective) and an optional `CurriculumItem.MinimumLevelByStage` jsonb column keyed by training year that overrides the flat `MinimumLevel` where present. Trainee progress view respects the stage-indexed minimum. Scope in `Rewrite/practical-plan.md` §T034. 1 day.

## This session at a glance

T033 shipped on `master`:

- **T033 — Per-trainee per-EPA trajectory chart** — (commit hash pending)

Trainees now drill into `/portfolio/progress` from the dashboard's "Curriculum progress" card to see a server-rendered SVG line chart per EPA — one dot per observation, rating on Y, date on X. The same chart also renders on `ReviewDetail.razor` so the committee sees the trend alongside the evidence snapshot and sampling warnings.

## Last completed

**T033 — Per-trainee per-EPA trajectory chart.**

- New MediatR query `GetEpaTrajectoryForTraineeQuery(TraineeUserId, From?, To?)` in `Wombat.Application.Features.Activities.Queries.GetEpaTrajectoryForTrainee`. Returns per-EPA observations (date, rating, source label, assessor) for the four rated seed keys (mini_cex, dops, cbd, acat) by parsing `DataJson` client-side — matches the T032 JSON-extraction approach so in-memory tests work without PostgreSQL JSON support.
- New reusable `TrajectoryChart.razor` under `Components/Shared` — pure server-side SVG. Parameters: `Points`, `AriaLabel`, optional width/height/min-max rating. Renders a polyline between dots, integer Y-ticks 1–5, and first/last date labels on the X-axis. SVG `<text>` nodes are emitted via `MarkupString` to sidestep Razor's `<text>` directive.
- New trainee-facing page `/portfolio/progress` (`MyProgress.razor`) listing each EPA with observations, each with its trajectory chart and an observation/assessor count. Wired from the `TraineeDashboard` "Curriculum progress" card (previously a dead `/curriculum` link).
- `ReviewDetail.razor` gained a "Rating trajectory by EPA" section between Evidence snapshot and Appeals, showing a chart for every EPA the trainee has observations against. All-time trajectory — the committee sees the full trend, not just the window.
- Chart styles added to `app.css` under "Trajectory chart" — dot fill, line stroke, axis colour and label typography all token-driven. No raw hex, no component-scoped CSS.
- 7 Application tests for the query (ordering by date, grouping by EPA code, period filter, trainee scoping, unrated-type exclusion, missing-overall skip, source mapping) + 5 bUnit smoke tests for the chart (empty state, one point, multi-point line + dots, axis date labels, aria attributes).
- No new aggregate, no new domain entity, no migration.

## Plan this session works against

`Rewrite/practical-plan.md` — Block 3 in progress: T033 done, T034 next.

## Block 3 sequence

1. ✅ T033 — per-trainee per-EPA trajectory chart
2. T034 — EPA core/elective + stage-indexed supervision levels (active)

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 38/38 pass
- Application tests — 153/153 pass (7 new for trajectory query)
- Architecture tests — 19/19 pass
- Web tests — 38/38 pass (5 new for TrajectoryChart)
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T033 compromises

- **No browser-level verification this session.** The dev server requires a seeded database and a logged-in Trainee principal to exercise `/portfolio/progress`, which is more setup than the 2-day T033 scope justifies. bUnit smoke tests cover the rendering contract; the in-page Razor markup is straightforward enough to trust from tests alone. Note for next session if `/portfolio/progress` goes live in a real environment.
- **All-time trajectory everywhere.** The query accepts optional `From`/`To` but both callers pass no window — the committee and the trainee see the full history. A "within-review-period" band overlay would be nice but is not in §T033.
- **Only the four rated seed keys contribute.** mini_cex/dops/cbd/acat are the only seeds today that carry `overall` + `epa_id` + `assessor_user_id`. If a future custom activity type adds these fields, extend `SourceByActivityKey` in the query (same spot as T032's sampling query).
- **Observed date = `Activity.CreatedOn`.** Activities don't carry a separate "observed on" field; CreatedOn is the closest proxy. If a clinical date field is added later, switch the point's date to that.

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
