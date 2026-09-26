---
id: T296
title: Docs say nine roles (the code has ten), DOMAIN's glossary defines STAR wrongly, and STATE miscounts the queue
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
started: 2026-09-26
completed: 2026-09-26
---

# T296 — Docs say nine roles (the code has ten), DOMAIN's glossary defines STAR wrongly, and STATE miscounts the queue

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Wrong in the documents agents load first; the redesign's role list would have missed CollegeAdmin.
**Surfaced:** 2026-09-26, the read-only inventory made for the Claude Design question.

## Symptom

- CLAUDE.md and DOMAIN.md say nine roles; `WombatRoles.All` has ten (CollegeAdmin, T093).
- DOMAIN.md's glossary defines STAR as Situation-Task-Action-Result; since T028/T029 it is the Statement of Awarded
  Responsibility. Its role table has SpecialityAdmins defining curricula and admitting trainees; the College authors
  curricula (T091, T093) and an InstitutionalAdmin admits (`AdmitTraineeCommand`).
- STATE.md says 50 tasks are queued; `queued/` held 38.

## Verification

- [x] CLAUDE.md lists ten roles and the former-trainee claim — read, 2026-09-26 (§ Roles; § Key technical choices'
  Auth row).
- [x] DOMAIN.md: ten roles, CollegeAdmin row, admission by an InstitutionalAdmin, STAR glossary — read. The role
  table's InstitutionalAdmin, SpecialityAdmin and Trainee rows now say what the product lets them do.
- [x] STATE.md's queue count matches `ls execution/tasks/queued | wc -l` — 39 on 2026-09-26. The session's final STATE
  rewrite restates it.

## As built — 2026-09-26

Also corrected: `knowledge/README.md` named decisions D1–D38 twice; they run to D50.
