---
id: T204
title: The dev seed has no Coordinator, so committee scheduling and MSF campaigns cannot be exercised with a tracked dev account
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
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

- [x] A fresh dev boot creates the Coordinator; a second boot changes nothing. Seeder test.
- [x] Browser (T167, left open): as the Coordinator, schedule a review for Demo Trainee on panel 1 (window 2026) and
      start it as the chair. The snapshot shows one card per EPA in code order, grouped by instrument, with rung
      ratings and encounter dates; MSF per-EPA records sit under their EPAs; campaign reports sit under "Not about a
      single EPA".
- [x] Browser (T184, left open): in a new draft campaign, the AddInvitation audit row reads `"respondentEmail":"[REDACTED]"`.
      Opening it shows "Campaign opened, and each respondent has been emailed a link to respond." Closing it
      anonymises each invitation (`AnonymizedOn` = `ClosedOn`).

## Related

T130 (`DevUserSeeder`), T167, T184, T182.

---

## As built — 2026-09-24

`DevUserSeeder` seeds `coordinator@wombat.local` ("Demo Coordinator", Coordinator, DEMO institution, scoped to General
Medicine and Paediatrics) with a dev-only constant. It is Development only, like the other dev users. Commit `02336bc`.

**Browser on dev (scripted Chrome):**
- **Seeding.** The first boot logged "Seeded dev Coordinator user coordinator@wombat.local." The second logged
  nothing, and the user's row, roles, scopes and stamps were byte-identical.
- **T167.** As the Coordinator, review 2 was scheduled (panel 1, Demo Trainee, 2026). It was started as the chair
  (32 snapshot items).
  - Cards follow code order: PAED-001, 002, 004, 005, 006, 007, 010, 011, 012, then "Not about a single EPA".
  - PAED-001 groups CCA #19 (3a); Mini-CEX #7–#9, #12, #13 (3b), #16 (3a), and #15, #18, #21 ("Not recorded";
    cancelled, declined, and draft with "(filed; no encounter date)"); MSF #1 (3b); and Reflective Exercise #32
    (Unrated).
  - The MSF per-EPA records sit under their EPAs, and the three campaign reports under "Not about a single EPA".
- **T184.**
  - Campaign 5, invitation 14: the audit row reads `"respondentEmail": "[REDACTED]"`, with no "@" anywhere.
  - Open: "Campaign opened, and each respondent has been emailed a link to respond." The send failed as expected on
    dev (no SMTP).
  - Close: "Campaign closed and anonymised for review." The invitation's email is null, its hashes are present, and
    `AnonymizedOn` equals `ClosedOn`.

