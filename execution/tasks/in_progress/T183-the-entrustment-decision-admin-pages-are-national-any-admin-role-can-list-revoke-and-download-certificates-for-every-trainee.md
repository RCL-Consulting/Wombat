---
id: T183
title: The entrustment-decision admin pages are national: any admin role can list, revoke and download certificates for every trainee
status: in_progress
priority: P1
owner: agent
depends_on: []
created: 2026-09-24
started: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

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

- [ ] An admin role at institution A cannot list, revoke or download for a trainee at B. Handler tests; a revoke changes
      nothing.
- [ ] Administrator and the subject still can. Handler tests.

## Related

T113, T056, T101.
