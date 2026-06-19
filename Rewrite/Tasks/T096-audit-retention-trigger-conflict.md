# T096 — Audit retention job vs. append-only trigger conflict

**Surfaced by:** T015 production deployment (2026-06-19), reviewing the audit append-only
enforcement before applying the (redundant) manual `REVOKE`.

## Problem

The `AuditLog` migration installs a PostgreSQL trigger `audit_entries_immutable` whose
function `prevent_audit_entry_mutation()` raises an exception on **any** UPDATE or DELETE
of `AuditEntries`, for every role (triggers fire regardless of role). This correctly makes
the live audit table append-only.

But `AuditLogRetentionJob` (`Wombat.Infrastructure.Scheduling.Jobs`) archives entries older
than 2 years by `AddRange`-ing them into `AuditEntryArchives` and `RemoveRange`-ing them from
`AuditEntries` — i.e. it **DELETEs** from `AuditEntries`. The trigger blocks that delete, so
the job would throw the moment any row ages past 2 years.

Dormant on a fresh DB (nothing is 2 years old; a manual "Run now" archives 0 rows and never
reaches a delete), so it never surfaced in the scenario replays — but it is a real latent bug
that would break audit retention in production in ~2 years.

## Fix (shipped 2026-06-19)

Keep the table append-only for every normal path, but let the **sanctioned archival mover**
opt its transaction in via a session-local custom GUC.

- **Migration `T096_AuditDeleteForArchival`** — `CREATE OR REPLACE` the trigger function so a
  DELETE is permitted only when `current_setting('wombat.allow_audit_delete', true) = 'on'`.
  All UPDATEs and all other DELETEs still raise. (`wombat.*` is a custom GUC namespace any role
  can `SET LOCAL` — no superuser needed.)
- **`AuditLogRetentionJob`** — resolve the concrete `ApplicationDbContext` (Infrastructure-
  internal) and wrap each archival batch in an explicit transaction that runs
  `SET LOCAL wombat.allow_audit_delete = 'on'` before `SaveChangesAsync`, then commits. The
  GUC is transaction-scoped, so it never leaks to other code paths.

No model change (SQL-only migration; model snapshot untouched).

## Verification

- [x] Trigger logic validated on a throwaway restore of the production DB (Postgres 18):
  delete without the GUC → blocked; UPDATE with the GUC → still blocked; delete **with** the
  GUC → succeeds (row removed). 
- [x] Migration applied to production on restart; `pg_get_functiondef` confirms the live
  function is GUC-gated. App healthy.
- [x] `dotnet build -c Release` clean.
- [ ] End-to-end job run against aged rows — not exercised (fresh DB has none); will run for
  real at 03:00 once rows age, or test in staging by back-dating a row's `OccurredAt`.

## Notes

The 7-year cold-storage step (INFRASTRUCTURE.md) deletes from `AuditEntryArchives`, which has
**no** immutability trigger, so it is unaffected. Only `AuditEntries` is trigger-protected.
