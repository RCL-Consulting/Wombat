---
id: T290
title: Trainee-first and existence leftovers: completing your own profile, invitations, the Users commands' refusals, and two committee queries that read claims
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T290 — Trainee-first and existence leftovers: completing your own profile, invitations, the Users commands' refusals, and two committee queries that read claims

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T278, T279 and T281 reviews.

## Items

- `CompleteTraineeProfile` removes the caller's own Trainee role, and `/admin/trainees` has no trainee-first check.
- Issuing an invitation, revoking one by id, and T283's ResendInvitation have no trainee-first check (T278 guards only the
  Users feature).
- The Users commands reveal whether an id exists. Add role, remove role and password reset answer "The user could not be
  found." where scope would refuse. Add and remove role, and `SetUserLockout`, give the global-administrator refusal
  before the scope check. Use one refusal before any state check (T194's rule).
- `ListReviewsForChairQuery` is unused and ignores the review access rules. Delete it.
- `ListDecisionPanels` still lists panels from the caller's claims, not the seat rule.
- **Withdrawn trainees and the Trainee role.** Nothing warns while an after-end encounter date is typed (T281 refuses
  credit, but the form does not say so), and a withdrawn trainee keeps the Trainee role. Decide whether they should.

## Verification

- [ ] Each item fixed has a test, or its decision is recorded.

## Related

T278, T279, T281, T194, T185.

Note, 2026-09-25 (the final browser check): a circuit whose tab has gone still runs T279's revalidation after a role
change, and logs "Navigation failed … TaskCanceledException" and "Unhandled exception in circuit" at error level. It is
harmless, but it is noise: treat a cancelled navigation on a gone circuit as expected.
