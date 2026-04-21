# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T032 — Sampling-concentration warning on review detail.** Model: Sonnet.

Second Block 2 task. A single query executed from `ReviewDetail.razor` that reports on the evidence distribution for the trainee under review: one-assessor-over-50%, single-source-only, fewer-than-three-distinct-assessors. Rendered as a dismissible warning panel. No new aggregate, no projection table. See scope in `Rewrite/practical-plan.md` §T032. 1–2 days.

## This session at a glance

Block 1 closed and Block 2 half done. Three tasks shipped back-to-back on `master`:

- **T029 — `EntrustmentDecision` aggregate (STAR)** — `91ff841`
- **T030 — STAR certificate PDF + authorisations UI** — `10f7e55`
- **T031 — formative-only committee review mode** — `ac4fdb9`

Hospital now has an end-to-end defensible entrustment record: committees ratify, decisions issue atomically, trainees download QuestPDF certificates, admins revoke with audit, and interim formative check-ins run without producing binding paperwork.

## Last completed

**T031 — Formative-only committee review mode.**

- `CommitteeReview.IsFormative` bool + `Close(actor, utcNow)` domain method. Formative reviews transition `Scheduled` → `InProgress` → `Final` via `Close`; they cannot `RecordDecision` or `Ratify` (both throw).
- Migration `20260421152754_FormativeCommitteeReviews` adds the boolean column (default false).
- `ScheduleCommitteeReviewCommand` gained an `IsFormative` parameter (default false, non-breaking); new `CloseFormativeReviewCommand` uses `DemandChairAccess`.
- `StagePendingEntrustmentDecisionCommand` now rejects formative reviews up front — entrustment decisions cannot be issued against interim check-ins.
- Review detail DTOs propagate `IsFormative` through all list queries (panel, trainee, chair).
- UI: `ReviewsSchedule.razor` has a "Formative only" checkbox and the listing shows a Mode column. `ReviewDetail.razor` hides the decision-recording, pending-entrustment, and appeals sections for formative reviews and replaces Ratify with a "Close review" button that fires from `InProgress`.
- 5 new Domain tests (Close happy path, RecordDecision/Ratify throw, summative cannot Close, cannot Close from Scheduled) + 5 new Application handler tests (schedule persists flag, close from InProgress, non-chair rejected, record decision blocked, staging pending blocked).

## Plan this session works against

`Rewrite/practical-plan.md` — the pragmatic post-rewrite plan. Four blocks, nine tasks (T028–T036). Block 1 done. Block 2 has two tasks: T031 done, T032 remaining.

## Block 2 sequence

1. ✅ T031 — formative-only committee review mode
2. T032 — sampling-concentration warning on review detail (active)

## Test status at handoff

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- Domain tests — 38/38 pass (5 new for formative)
- Application tests — 139/139 pass (5 new for formative)
- Architecture tests — 19/19 pass
- Web tests — 33/33 pass
- Infrastructure tests — `SeedParseTests` pre-existing parallel-run flakiness; passes in isolation
- Integration tests — Docker-gated; not run locally

## Known T031 compromises

- **`Close` reuses `CommitteeReviewState.Final`** rather than adding a `Closed` state. The state enum was not expanded; `IsFormative` tells you whether `Final` was reached via ratification or via a formative close. Adding a `Closed` state later is additive and non-breaking.
- **Domain changes reverted by a Razor-format linter during editing.** `ReviewDetail.razor` was rewritten in full rather than edited incrementally because sequential `Edit` calls kept hitting "file modified since read" failures from a format pass after each edit. Future Razor edits should consider a single Write or pre-read immediately before each Edit.

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

- `ac4fdb9` — T031 (formative-only committee review mode)
- `c9b00d0` — docs: record T030 commit hash
- `10f7e55` — T030 (STAR certificate PDF + authorisations UI)
- `21f7959` — docs: record T029 commit hash in current_state handoff
- `91ff841` — T029 (EntrustmentDecision aggregate / STAR)
- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)
