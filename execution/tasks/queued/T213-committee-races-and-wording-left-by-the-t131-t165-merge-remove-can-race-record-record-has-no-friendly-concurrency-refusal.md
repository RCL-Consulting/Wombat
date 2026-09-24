---
id: T213
title: Committee races and wording left by the T131/T165 merge: Remove can race Record, Record has no friendly concurrency refusal
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# T213 — Committee races and wording left by the T131/T165 merge: Remove can race Record, Record has no friendly concurrency refusal

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A race can break "staged STARs are fixed once the decision is recorded" (D46).
**Surfaced:** 2026-09-24, the T165 + T131 merge (gaps neither lane guarded).

## Items

1. **Remove can race Record.** Remove does not mark the review as modified. A remove that read the review while it was
   in progress can still delete a staged decision after Record commits. Make Remove touch the review's concurrency
   token, as Stage does.
2. **Record has no friendly refusal.** If a stage commits between Record's read and its save, Record fails with a raw
   concurrency error. Translate it into a readable message, as Stage and Ratify do.
3. **Two messages say "Remove it and stage again"**, one on the review page and one in
   `StagedEvidence.DemandGroundedAsync`, about a decision with no evidence. Since D46 that cannot happen on a decided
   review. Reword or remove them.
4. **The remit form's comment** says the chair and the resolver are ticked and locked, but the code locks the chair plus
   the current user. Make the comment and the code agree.

5. **Two refusals show the raw validator format**, "Validation failed: -- PresentUserIds: … Severity: Error" (record
   decision) and "Validation failed: -- Members: …" (panel). Surface the message only.
6. **A non-chair is offered Record and Ratify**, which then refuse. Offer the chair's controls to the chair only
   (picker = gate).

## Verification

- [ ] A Remove racing a Record is refused whole. Postgres race test.
- [ ] Record racing a stage gets a readable refusal. Test.

## Related

T131, T165, D46.
