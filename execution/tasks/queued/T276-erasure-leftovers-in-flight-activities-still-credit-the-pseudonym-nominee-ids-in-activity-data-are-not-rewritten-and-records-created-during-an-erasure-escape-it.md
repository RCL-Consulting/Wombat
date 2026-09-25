---
id: T276
title: Erasure leftovers: in-flight activities still credit the pseudonym, nominee ids in activity data are not rewritten, and records created during an erasure escape it
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T276 — Erasure leftovers: in-flight activities still credit the pseudonym, nominee ids in activity data are not rewritten, and records created during an erasure escape it

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. There are no real users (W-007), but the design should be right.
**Surfaced:** 2026-09-25, the T258 review (findings 7 and 8). Listed in CUSTOMIZATION.md § 5 as not yet ended by an
erasure.

## Symptom

- **In-flight activities.** Activities about the erased trainee that are still in their workflow can be completed and
  credited to the pseudonym. `CreditTargetResolver` does not filter on an active profile.
- **Nominee ids.** User ids stored in activity `DataJson` nominee fields are never rewritten.
- **Respondent addresses.** An erased person's email as a respondent on someone else's open campaign stays until that
  campaign closes.
- **Concurrency.** The erasure reads under READ COMMITTED and locks nothing, so a review, campaign or profile created
  during it lands under the original id.

## What to build

- End in-flight activities at erasure: withdraw or cancel them through the workflow, recording why.
- Rewrite nominee ids in `DataJson`, through `ActorFieldRules`, the one walker.
- Anonymise the person's respondent invitations on other campaigns.
- Make scheduling, campaign creation and admission take the same user-row lock the erasure takes, or run both sides
  SERIALIZABLE.

## Verification

- [ ] After erasure, no in-flight activity credits the pseudonym, no nominee field holds the original id, and no
      invitation holds the address. Postgres tests with the real `ErasureExecutor`.

## Related

T258, T238, T026, T207.

Notes, 2026-09-25 (the lifecycle browser check): erasure also leaves the person's address in two places:
- `Invitations.Email`, on the invitation addressed to them. The executor rewrites only invitations the person issued.
- `DataRightsRequests.RequesterDisplayName`, which `/admin/data-rights` shows after completion.
