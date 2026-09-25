---
id: T268
title: A locked trainee still counts as current for committee scheduling and MSF, although a locked member cannot sit on a panel
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T268 — A locked trainee still counts as current for committee scheduling and MSF, although a locked member cannot sit on a panel

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The two rules disagree about the same person.
**Surfaced:** 2026-09-25, the T248 review (finding 2).

## Symptom

T238's current-trainee rule (`TraineeScopeResolver.KeepCurrentAsync`, `HoldersAsync`) reads only role links, so an
account an admin has locked (`UserDeactivation.IsDeactivated`) still counts as a current trainee. `PanelSeat` excludes
deactivated members (T165, D46).

## Decision (adopted 2026-09-25; the operator may overrule)

A locked account is not current. The same reason applies as for panel seats: an admin's lock means "not working here
now". A locked trainee's existing records stay readable to staff.

## What to build

Exclude `UserDeactivation.IsDeactivated(LockoutEnd)` in `KeepCurrentAsync`, the one rule, and add a locked case to its
tests and to T238's.

## Verification

- [x] A locked trainee is offered to no picker, and scheduling and MSF create refuse them. Tests.

## Related

T238, T248, T165, D46.

---

## As built — 2026-09-25 (`d42d2db`, decision adopted)

`TraineeScopeResolver.KeepCurrentAsync` excludes an account an administrator has locked (not a wrong-password lockout),
as a locked member cannot sit on a panel. So pickers, scheduling, MSF create, decisions-due, coverage and dashboards leave
them out. Tests pin a locked twin of each.

Browser on dev (scripted Chrome, master `3f08b26`; `pg_dump -n public` first, at `recovery/pre-t251-migration.dump`):
- **Locked by instadmin.** Stale schedule and MSF forms were refused with their exact sentences, and nothing was stored.
  After a reload the pickers, decisions-due, coverage and the targets card left the trainee out. Their existing
  campaigns and reviews still open.
- **Reactivated:** every item came back.
- **Five wrong passwords** (a lockout, not an admin's lock) left the trainee current everywhere.

**Filed from the review:** [T284] (opening a draft about someone no longer current; the digest's inactive list; the lock
card).
