---
id: T201
title: When a handler's own save fails, the audit pipeline re-saves the same changes, loses the audit row and hides the real error
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

**Severity:** Medium. A failed write leaves no audit record, and the user sees EF's raw message instead of the cause.
**Surfaced:** 2026-09-24, the T184 review (confirmed on Postgres with a concurrency conflict on an MSF open).

## Symptom

After a `DbUpdateException` in a handler's own `SaveChangesAsync`, `AuditPipelineBehavior`'s catch calls
`AuditWriter.WriteAsync`. That shares the scoped DbContext, still holding the rejected changes, and saves again. The
save throws again, so the audit row is lost and EF's message replaces the original exception.

## What to build

When the handler's save failed, clear the change tracker (or use a separate context) and write only the audit row,
then rethrow the original exception. Keep the existing "mutate-then-throw commits" behaviour for exceptions that are
not save failures only if it is still wanted: the audit-trap memory says it is the known trap. Decide, and record it.

## Verification

- [x] A concurrency conflict in a handler leaves an audit row with Success=false and surfaces the original error.
      Postgres test.

## Related

T184, the audit-trap memory, T096.

---

## As built — 2026-09-24

**Decision, recorded here and in memory:**
- When the handler's own save is refused (`DbUpdateException`, found on the exception) or the failure write is refused,
  `AuditPipelineBehavior` drops the pending changes and writes the audit row alone (Success=false). It then rethrows
  the handler's own exception.
- For any other exception, the known trap is unchanged: the failure write can still commit a change the handler made
  before throwing, so checks still come first.
- A refused success-row write is not recorded as a failed command.

Tests: unit tests, plus Postgres tests for a concurrency conflict inside `UserManager` and for a refused insert whose
clash has since gone. Mutation-checked.

Filed: [T208] (the User-Agent and display-name lengths).
