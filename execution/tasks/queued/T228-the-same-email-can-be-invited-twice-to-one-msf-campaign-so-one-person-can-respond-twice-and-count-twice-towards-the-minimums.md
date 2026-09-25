---
id: T228
title: The same email can be invited twice to one MSF campaign, so one person can respond twice and count twice towards the minimums
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T228 — The same email can be invited twice to one MSF campaign, so one person can respond twice and count twice towards the minimums

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. One respondent can fill a group's minimum alone, and each copy of the invitation is mailed its own
working link.
**Surfaced:** 2026-09-25, the F2 browser check (campaign 11 on dev: two addresses each invited twice, five invitations,
two mails each in the sink).

## Symptom

`AddMsfInvitationCommandHandler` (`AddMsfInvitation.cs`) has no duplicate check. Adding an address that the campaign
already invites creates a second invitation, and opening the campaign mails both.

## What to build

Refuse an address the campaign already invites (compared case-insensitively after trimming), before any write, with a
refusal that does not echo other invitees. Consider a unique index on the campaign and a normalised address while the
address is held. The address is nulled at anonymisation (T207), so the index must allow that. A refusal on the page is
announced as T217's refusals are.

## Verification

- [ ] A second invitation of the same address, in any case, is refused and nothing is written. Handler test (and the
      audit trap: nothing committed on refusal).
- [ ] Two concurrent adds of one address leave one invitation. Postgres test.
- [ ] Browser: adding the same address twice shows the refusal.

## Related

T217, T205, T207, T163.
