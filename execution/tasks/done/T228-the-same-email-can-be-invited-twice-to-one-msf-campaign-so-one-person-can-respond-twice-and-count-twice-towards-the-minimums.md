---
id: T228
title: The same email can be invited twice to one MSF campaign, so one person can respond twice and count twice towards the minimums
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] A second invitation of the same address, in any case, is refused and nothing is written. Handler test (and the
      audit trap: nothing committed on refusal).
- [x] Two concurrent adds of one address leave one invitation. Postgres test.
- [x] Browser: adding the same address twice shows the refusal.

## Related

T217, T205, T207, T163.

---

## As built — 2026-09-25 (`7b6d700`)

A campaign invites an address once, compared trimmed and case-insensitively.
- **The rule.** `AddMsfInvitation` refuses a repeat after the scope check and before any write. The refusal never echoes
  the address, and the audit row redacts it.
- **The database.** The migration `T228_MsfInvitationAddressOnce` adds a stored generated `RespondentEmailKey` and a
  unique index on campaign and key, filtered to held addresses. It first removes existing duplicates, keeping the copy
  that responded.
- **The page** sets an `_adding` flag before its first await, so a double press sends nothing twice.

Tests: handler (scope first; the audit trap), Postgres concurrency, the migration's duplicate cases, and bUnit.

Browser on dev (scripted Chrome, master `ec58d2e`; `pg_dump` first, at `recovery/pre-g2-migrations.dump`):
- **Campaign 13.** Refused: "PEER-A@Example.test" (Enter), the same by click, and the address with spaces. Each refusal
  was `role=alert` "This campaign already invites that address…", with no address named and the focus kept.
- **Double presses.** A double-click and a double Enter each added exactly one invitee. The audit rows read
  `respondentEmail: [REDACTED]`.
- **Campaign 14.** Three invitees produced exactly three mails.
- **Duplicates.** None remain after the migration (dev's were on withdrawn campaign 11, already anonymised).

**Filed from the review:** [T247] (remove a draft invitee). The rule is one invitation per learner per campaign,
whatever the teaching context. That bears on § 3F question 9.
