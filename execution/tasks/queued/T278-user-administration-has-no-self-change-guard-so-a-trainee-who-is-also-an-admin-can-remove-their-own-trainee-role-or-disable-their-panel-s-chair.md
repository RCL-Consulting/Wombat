---
id: T278
title: User administration has no self-change guard, so a trainee who is also an admin can remove their own Trainee role or disable their panel's chair
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T278 — User administration has no self-change guard, so a trainee who is also an admin can remove their own Trainee role or disable their panel's chair

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. It is the lever that undoes T256's trainee-first rule.
**Surfaced:** 2026-09-25, the T256 review (out of scope 1).

## Symptom

`RemoveRoleFromUserCommand` checks only Administrator protection and institution scope. `AddRoleToUserCommand` and
`SetUserLockoutCommand` follow the same pattern, and `UserAdministrationRules.AssignableRoles` includes Trainee.
- A Trainee who is also an InstitutionalAdmin can remove their own Trainee role, sign in again, and administer the panel
  that reviews them.
- Since T256 every chair action reads the user store, so the same person can remove CommitteeMember from their own
  panel's chair, or lock the chair out, and so stop the ratification of their own review.

## What to build

- Refuse changing your own roles and your own lockout.
- Refuse user administration to anyone who holds Trainee (T185's trainee-first rule), with the Users pages saying why.
- Consider also refusing changes to the members of any panel that reviews the caller.

Handler tests per command (the audit trap); bUnit for the pages.

## Verification

- [ ] Nobody changes their own roles or lockout, and a Trainee administers no user. Tests.

## Related

T256, T185, T237, D46.
