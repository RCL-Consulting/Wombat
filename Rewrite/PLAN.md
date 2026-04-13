# Master Plan

## Goal

Rewrite Wombat on the ClinicAssist.NET architecture (Clean Architecture + CQRS/MediatR + Blazor Interactive Server + EF Core 10 + PostgreSQL + ASP.NET Core Identity), deployed to Linode (Ubuntu 24.04 + Caddy + systemd), **preserving the original Wombat intent that admins can add new activity types (WBAs, reflections, research outputs, teaching logs, QI projects, anything) without developer involvement.**

The plan is structured in two parts: a **core** (T001–T016) that stands the platform up, and an **extension** (T017–T027) that adds the customization engine, the real-world features identified during product evaluation, and the operational concerns (audit, data rights, SSO, deployment hardening). The extension is not optional; it is where the difference between "proof of concept" and "real tool" lives.

## Success criteria

1. `dotnet build` clean across all projects, no warnings above chosen severity.
2. `dotnet test` green — unit, architecture, integration smoke tests.
3. An admin can log in, define a new activity type called "Journal Club Attendance" through the UI, and a trainee can immediately log one, without any code being written or deployed.
4. A user can go through invitation → registration → role admission → curriculum assignment → activity log → curriculum progress update → committee decision → portfolio PDF export as a complete lifecycle.
5. The application runs under systemd on Linode, fronted by Caddy with TLS, talking to PostgreSQL, sending email via MailKit.
6. POPIA/GDPR data subject requests (export, deletion) are self-service.
7. Admin actions are audit-logged. Audit log is append-only.
8. All of the above is reachable from a fresh clone in one `deploy/README.md` run.

## Phase structure

### Phase 0 — Ground truth

- [x] T001 — Scaffold the solution
- [x] T002 — Port Identity & role model
- [x] T003 — Institutions, Specialities, SubSpecialities

### Phase 1 — Core domain

- [x] T004 — EPAs, Curricula, assessment forms *(note: `AssessmentForm` from T004 stays as the scale/rubric building block; specific instruments move to activity types in Phase 3)*
- [x] T005 — Invitation flow
- [x] T006 — Trainee / Assessor / Admin profile data

### Phase 2 — Activity platform *(NEW — replaces the former Phase 2 assessment aggregates)*

- [x] T017 — Activity platform: schema, aggregates, storage
- [x] T018 — Activity engine: generic commands, workflow runtime, credit rules
- [x] T019 — Activity builder UI + dynamic form renderer (v1 — visual form editor, JSON-validated workflow/credit editors, ten field types, single-condition `show_if`, draft/publish lifecycle)

> The former T007, T008, T009 are superseded. Their files remain for history. Do not execute them.

**Deferred to post-launch follow-ups** (each small, independent, non-blocking — do not pull any of these into T019):

- [ ] T019-b — Drag-and-drop reordering of sections and fields
- [ ] T019-c — Nested sections + repeatable sections (e.g. PDSA cycles)
- [ ] T019-d — Visual workflow editor (drag-and-connect state machine designer)
- [ ] T019-e — Visual credit-rules editor (guided wizard with plain-English summaries)
- [ ] T019-f — Multi-condition visibility (`show_if` with AND/OR grouping)
- [ ] T019-g — Schema templates, copy-from-existing, advanced signature capture

### Phase 3 — Seeded activity types

- [x] T020 — Seed initial activity types (Mini-CEX, DOPS, CbD, ACAT, STAR Reflection, Procedure Log entry, Research Output, Teaching Session, QI Project)

### Phase 4 — Hardcoded domain features (things that cannot be no-code)

- [x] T021 — Multi-source feedback (MSF/360) with anonymity
- [ ] T022 — Committee decisions (ARCP equivalent)

### Phase 5 — Web & messaging

- [x] T010 — Web layout, auth, navigation, role-gated routing
- [ ] T011 — Role dashboards (widgets query generically over activities)
- [ ] T012 — Email infrastructure
- [ ] T024 — Scheduled nudges & digest emails

### Phase 6 — Cross-cutting operations

- [ ] T023 — Portfolio PDF export
- [ ] T025 — Admin audit log (pipeline behaviour)
- [ ] T026 — Data subject rights (POPIA/GDPR self-service)
- [ ] T027 — Institutional SSO (OIDC)

### Phase 7 — Quality & ship

- [ ] T013 — Architecture tests *(extended to cover the activity platform invariants)*
- [ ] T014 — Seeding & first-run bootstrap
- [ ] T015 — Linode deployment
- [ ] T016 — Smoke test, handover, delete old Wombat source

## Dependency graph

```
T001
 ├─ T002 ── T003 ── T004 ── T005 ── T006
 │                                  │
 │                                  ▼
 │                               T017 ── T018 ── T019 ── T020
 │                                                        │
 │                                                        ├─ T021 (MSF, parallel)
 │                                                        ├─ T022 (Committee, parallel)
 │                                                        │
 ├─ T010 (parallel from T002) ── T011 (needs T019, T021, T022)
 │                                  │
 │                                  ├─ T012 ── T024
 │                                  ├─ T023 (PDF, parallel)
 │                                  ├─ T025 (audit, parallel, needs T017)
 │                                  ├─ T026 (data rights, parallel)
 │                                  └─ T027 (SSO, parallel)
 │                                                        │
 │                                                        ▼
 └────────────────────────────────────────── T013 ── T014 ── T015 ── T016
```

T017 is the new critical path node. Nothing in Phases 3–6 that touches activity data can begin until the platform exists. T010 (web chrome) and T012 (email) can proceed in parallel from Phase 0/1 since they do not depend on the activity model.

## What changed from the first-draft plan

This plan replaces an earlier draft in which T007–T009 built concrete typed aggregates for Assessment, STAR, etc. Those tasks are superseded. The pivot happened when it became clear that the original Wombat's `Option`/`OptionSet` abstraction was a form-builder attempt and that admins are expected to add activity types without a developer. See `CUSTOMIZATION.md` for the reasoning and the schema-driven model.

## Non-negotiables

- No task is "done" until its verification section passes.
- No commit without updating `current_state.md`.
- No scope creep — unknown work becomes a new task file.
- Reference ClinicAssist freely for architecture questions, but understand the activity platform is a deliberate departure that ClinicAssist does not have. Do not copy ClinicAssist on anything that touches the activity model — that's where Wombat diverges.
- Platform stays generic. Resist the urge to add a typed aggregate for "that one annoying case". The annoying case is almost always another schema.
- Hardcoded exceptions (MSF, committee decisions, audit, data rights, SSO) stay in their own folders and do not leak into the activity platform. The line between platform and hardcoded is explicit and documented in `CUSTOMIZATION.md`.
