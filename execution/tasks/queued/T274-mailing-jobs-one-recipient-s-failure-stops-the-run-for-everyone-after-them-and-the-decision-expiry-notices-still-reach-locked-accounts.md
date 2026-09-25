---
id: T274
title: Mailing jobs: one recipient's failure stops the run for everyone after them, and the decision-expiry notices still reach locked accounts
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T274 — Mailing jobs: one recipient's failure stops the run for everyone after them, and the decision-expiry notices still reach locked accounts

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T117 and T240 reviews.

## Symptom

- **One failure stops the run.** In `WeeklyCoordinatorDigestJob`, an exception from the claims factory or the email
  sender for one coordinator stops the run for the rest. Check the other mailing jobs for the same loop shape.
- **Locked accounts still get expiry notices.** `EntrustmentDecisionExpiryJob` checks only for an email address, so an
  admin-locked trainee still gets "expired" and "expiring soon". T240's `ReminderRecipientPolicy` names this as a known
  gap.

## What to build

- Catch and log per recipient, and continue; count the failures in the run's log line.
- The expiry job skips a deactivated account (through the policy's deactivation half, not the opt-out, because an
  expiry notice is not a digest), and logs its skips by reason.

## Verification

- [ ] A throwing sender for one recipient leaves the rest mailed. Job tests.
- [ ] A locked trainee gets no expiry notice. Job test.

## Related

T117, T240, T151.
