---
id: T216
title: A user who is both Trainee and Coordinator can schedule and preview a peer's committee review
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T216 — A user who is both Trainee and Coordinator can schedule and preview a peer's committee review

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. It needs a user holding both roles. It is a gap in T185's "a Trainee is a trainee first" rule.
**Surfaced:** 2026-09-25, the lane B2 and batch E merge.

## Symptom

T185 made every surface that answers about other trainees ask `TraineeScopeResolver.ActsAsTrainee` first. Committee
scheduling (`DemandReviewScheduling`, `CommitteeTraineeScope.DemandSchedulableAsync`) and T131's
`PreviewCommitteeAgenda` do not ask it, so a user who is both Trainee and Coordinator can schedule, and preview the
agenda of, a peer's review.

## What to build

Apply the trainee-first rule to scheduling and the agenda preview (and the schedulable-trainee picker), or record a
decision that a trainee-coordinator may schedule, with the reason.

## Verification

- [x] A Trainee+Coordinator is refused scheduling and preview for a peer, and the picker offers them no one. Handler and
      bUnit tests.

## Related

T185, T182, T131.

---

## As built — 2026-09-25 (`3595589`)

A user who holds Trainee is a trainee first (T185) on committee scheduling, the agenda preview, the schedulable-trainee
picker and the review list. A Trainee with any role that schedules gets "You hold the Trainee role…", and a plain Trainee
the ordinary refusal. `GetCommitteeReviewsAccessQuery` tells the page whether to offer scheduling, so a plain
CommitteeMember is no longer offered it either. Handler tests cover each role pair; bUnit covers the page.

Browser on dev (scripted Chrome, master `225763e`): trainee was given Coordinator, then InstitutionalAdmin, then CommitteeMember, each through instadmin's Users page and
each removed afterwards.
- Each time, `/committee/reviews` had no Schedule button, and its empty card read "You hold the Trainee role, so you
  cannot schedule a committee review or preview its agenda, and this page lists no one's reviews…".
- A crafted `?panel=1&trainee=…&period=2026-2` opened no form.
- Decisions-due read "Nothing to show".
