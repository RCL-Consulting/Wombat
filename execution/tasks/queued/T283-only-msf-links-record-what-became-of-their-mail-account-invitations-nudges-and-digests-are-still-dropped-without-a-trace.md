---
id: T283
title: Only MSF links record what became of their mail: account invitations, nudges and digests are still dropped without a trace
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T283 — Only MSF links record what became of their mail: account invitations, nudges and digests are still dropped without a trace

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. An invitation to join Wombat that never arrives leaves an admin waiting.
**Surfaced:** 2026-09-25, the T251 review.

## What to build

- Extend T251's delivery outcome to account invitations (`IssueInvitation`): record sent or failed, show "not delivered"
  on the invitations list, and offer a Resend.
- Nudges and digests need only a count of failures in the job's log line.
- An address the mail server always refuses stays "not delivered" however often it is resent. Say so after the second
  failure, and suggest checking the address.

## Verification

- [ ] An undelivered account invitation is shown and can be resent. Tests.

## Related

T251, T184, T240.
