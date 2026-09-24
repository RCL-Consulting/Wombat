---
id: T163
title: Every MSF response scans every invitation ever issued
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T163 — Every MSF response loads and hashes every invitation in the database

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The result is correct, but the cost grows with every campaign ever run. Each respondent's page load and
each submission hashes the presented token against every invitation in the database.
**Surfaced:** 2026-09-24, the EPA-stream survey. It was [T121]'s adjacent defect 4, and no task carried it.

## Symptom

Observed at `431e69e`: `MsfCampaignRules.GetActiveInvitationByTokenAsync` (`MsfCampaignRules.cs:96-102`) loads every
`MsfInvitation`, with its campaign, template and questions, via `ToListAsync`. It then calls
`tokenService.VerifyToken(rawToken, candidate.TokenHash)` on each one in memory. There is no `Where` clause, and none
is possible while the hash is the only key.

## What to build

Give the lookup a key the database can index. Recommendation: a random, non-secret selector stored beside the hash and
carried in the link (`?s=<selector>&t=<token>`). Look the invitation up by selector, then verify the token against that
one hash in constant time. The alternative is a deterministic, indexable hash of the token (for example HMAC-SHA256
under a server key). Either way, the `TokenHash`-only link in any already-sent email stops working. Under W-007 that is
acceptable, but say so.

The link is built in one place, `WombatOptionsExtensions.RequireMsfRespondUrl`, shared by open and the reminder job
since [T132].

## Verification

- [ ] The token lookup issues one indexed query, not a full-table load. Test on the Postgres fixture, or a read of the
      generated SQL recorded here.
- [ ] A valid link still opens the form; a wrong token with a valid selector is refused; a used or revoked invitation is
      refused. Application tests.
- [ ] The expiry reminder's re-issued link works ([T132]'s tests stay green).
- [ ] Full suite green, no `--no-build`.

## Related

[T121], [T132] (the respond URL and token re-issue).
