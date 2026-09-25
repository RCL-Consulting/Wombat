---
id: T278
title: User administration has no self-change guard, so a trainee who is also an admin can remove their own Trainee role or disable their panel's chair
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Nobody changes their own roles or lockout, and a Trainee administers no user. Tests.

## Related

T256, T185, T237, D46.

---

## As built — 2026-09-25 (`2290ed5`)

- **Nobody changes their own account.** Nobody changes their own roles or lockout, and resets go through "Change your
  password".
- **Trainee first.** Anyone who holds Trainee administers no user: the Users pages show only a note, and every command
  refuses first.

Protecting the members of a panel that reviews the caller was considered and not adopted. Anyone under review while
holding Trainee is already refused. Covering a former trainee would mean widening `ActsAsTrainee` everywhere at once.
Handler and bUnit tests.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- **instadmin's own account** shows the note, no Remove, and no Add role, Reset or Lockout. committee's account is fully
  offered.
- **A trainee given InstitutionalAdmin** sees only the Trainee note on `/admin/users` and on any user page.
- **Confirmed, not fixed:** `/admin/trainees` still opens for that trainee (noted on [T290]).

**Filed from the review:** [T288] (P2: SSO group mappings across institutions), [T289] (P2: the assessor-profile form)
and [T290] (trainee-first leftovers).
