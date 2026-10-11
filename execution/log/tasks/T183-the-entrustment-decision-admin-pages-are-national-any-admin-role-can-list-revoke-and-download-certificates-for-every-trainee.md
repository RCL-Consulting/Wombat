---
id: T183
title: The entrustment-decision admin pages are national: any admin role can list, revoke and download certificates for every trainee
status: done
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
started: 2026-09-24
completed: 2026-09-24
---

# T183 — The entrustment-decision admin pages are national: any admin role can list, revoke and download certificates for every trainee

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** High. Revoking another institution's trainee's entrustment, and downloading their certificate, are
possible by role alone.
**Surfaced:** 2026-09-24, the T113 security review (finding 2, confirmed in code, outside T113's scope).

## Symptom

- `ListEntrustmentDecisionsForAdmin.cs:37-60` checks only the role, and `TraineeUserIdFilter` is a raw id.
- `EntrustmentDecisionAuthorization.DemandRevocationAccess`, called at `RevokeEntrustmentDecision.cs:59`, checks only
  the role.
- `DownloadEntrustmentCertificate.cs:52-58` checks only the role, and admits any Coordinator.

## What to build

Apply T056's rules through T113's resolver.
- **List:** filter by the caller's institution (Administrator sees all).
- **Revoke:** check the decision's trainee against the caller's institution before any mutation.
- **Certificate:** return nothing out of scope (404, not 403).

The subject may always download their own certificate. T113's architecture test
(`TraineeReadBoundaryTests`) already requires a principal on these requests; this task makes each handler use it.

## Verification

- [x] An admin role at institution A cannot list, revoke or download for a trainee at B. Handler tests; a revoke changes
      nothing.
- [x] Administrator and the subject still can. Handler tests.

## Related

T113, T056, T101.

---

## As built — 2026-09-24

`TraineeScopeResolver` has three nested predicates, each needing the trainee's institution:
- `IsAdministeredBy`: an InstitutionalAdmin, or a Speciality or SubSpecialityAdmin at the trainee's level.
- `IsAdministeredOrCoordinatedBy`: the above, plus a Coordinator.
- `IsOverseenBy`: the above, plus a CommitteeMember.

How each surface uses them:
- The admin list filters by `IsAdministeredBy`.
- Revoke needs Administrator, `IsAdministeredBy`, or the decision's **issuing** chair (`IssuedByChairUserId`, not
  current panel membership).
- The certificate needs the issuer or `MayReadAsync`.

At the merge with T182, Git's automatic resolution would have dropped T182's committee-member exclusion. The named
predicate fixes that, and a mutation test pins it.

Browser on dev, 2026-09-24 (master `b0cb335`, the batch-C check):  `/admin/entrustment-decisions` loads for the Administrator and filters by name. Dev holds 0 decisions, so
revoke and the certificate are covered by the handler matrix and Postgres tests, not the browser.
