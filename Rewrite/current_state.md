# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T029 — `EntrustmentDecision` aggregate.** Model: Opus.

The core artefact of Block 1: a per-trainee-per-EPA summative entrustment decision with rationale, evidence links, expiry, and revocation. Hard-coded aggregate (regulatory weight, immutability required). See scope in `Rewrite/practical-plan.md` §T029. Task spec file to be written as first step of the task.

## Last completed

**T028 — renamed `star_reflection` to `reflective_note`.** Build + tests green (194 tests). Migration `20260420120000_RenameStarReflection` ships the data rename for existing deployments. STAR acronym is now free for T029's formal authorisation artefact.

## Plan this session works against

`Rewrite/practical-plan.md` — the pragmatic post-rewrite plan. Four blocks, nine tasks (T028–T036), ~3–4 weeks total. Academic roadmap in `Rewrite/book-fidelity-plan.md` is superseded and kept only as reference for `EPA Book/critique.md`.

## Block 1 sequence

1. T028 — rename `star_reflection` → `reflective_note` (this task)
2. T029 — `EntrustmentDecision` aggregate
3. T030 — STAR certificate PDF + trainee "My authorisations" panel

## What was last verified (T016)

- `dotnet build Wombat.sln -c Release` — zero errors, zero warnings
- `dotnet test` — 191 tests green (17 Domain + 122 Application + 19 Architecture + 33 Web)
- All 8 PLAN.md success criteria traced through the codebase
- Zero TODO/HACK/FIXME markers in source

## What remains (operational, not code)

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

- `dc506d1` — T028 (rename `star_reflection` → `reflective_note`)
- `bf583ee` — MailKit 4.16.0 security bump (closes GHSA-9j88-vvj5-vhgr)
- `864ad3b` — T016 (rewrite-complete baseline)

Build + tests green at `dc506d1`: 194 tests across Domain, Application, Architecture, Web, and Infrastructure. Integration tests require Docker; skipped in Windows-dev environment per CLAUDE.md.
