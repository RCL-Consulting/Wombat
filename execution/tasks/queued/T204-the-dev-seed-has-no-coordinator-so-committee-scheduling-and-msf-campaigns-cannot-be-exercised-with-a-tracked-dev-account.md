---
id: T204
title: The dev seed has no Coordinator, so committee scheduling and MSF campaigns cannot be exercised with a tracked dev account
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium for verification. The committee and MSF flows are core to the EPA stream, and since T182 no
seeded dev account can schedule a review. Browser checks of T167's new snapshot and all of T184's MSF checks are
therefore still open.
**Surfaced:** 2026-09-24, the T167 and T184 browser checks.

## Symptom

`DevUserSeeder` seeds admin, assessor, committee (CommitteeMember + Assessor) and trainee. Scheduling a review needs
Administrator, InstitutionalAdmin, Coordinator, SpecialityAdmin or SubSpecialityAdmin (`MayScheduleReviews`, T182). MSF
campaign pages need Coordinator or Administrator. The only dev Coordinator (coordinator.t113b, institution 2) was made
by hand and has no recorded password, and verification agents may not read the admin credential.

## What to build

Seed `coordinator@wombat.local` (Coordinator at the dev institution, speciality Paediatrics) in `DevUserSeeder`, the same
way the other dev users are seeded: a dev-only constant, Development environment only. Consider an InstitutionalAdmin
too. Then run the checks left open.

## Verification

- [ ] A fresh dev boot creates the Coordinator; a second boot changes nothing. Seeder test.
- [ ] Browser (T167, left open): as the Coordinator, schedule a review for Demo Trainee on panel 1 (window 2026) and
      start it as the chair. The snapshot shows one card per EPA in code order, grouped by instrument, with rung
      ratings and encounter dates; MSF per-EPA records sit under their EPAs; campaign reports sit under "Not about a
      single EPA".
- [ ] Browser (T184, left open): in a new draft campaign, the AddInvitation audit row reads `"respondentEmail":"[REDACTED]"`.
      Opening it shows "Campaign opened, and each respondent has been emailed a link to respond." Closing it
      anonymises each invitation (`AnonymizedOn` = `ClosedOn`).

## Related

T130 (`DevUserSeeder`), T167, T184, T182.
