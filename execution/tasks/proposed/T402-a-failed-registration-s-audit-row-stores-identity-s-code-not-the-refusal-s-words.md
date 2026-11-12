---
id: T402
title: A failed registration's audit row stores Identity's code, not the refusal's words
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-11-12
---

# T402 — A failed registration's audit row stores Identity's code, not the refusal's words

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The audit log's Result column and an entry show developer wording to an administrator; nothing is
lost or leaked.
**Surfaced:** 2026-11-12, T400's replay, act 2 (step 2.8, Mr Patel's 10-character register attempt), read by SQL.

## Why

Found during T400's acceptance replay (flow 18): the audit log now shows a failed command's stored words (P1, F1), and
this row's words are not a refusal a person would read.

## Symptom

The `AcceptInvitationCommand` audit row for a too-short password stores "Identity refused the new account
(PasswordTooShort).", where the register page tells the person "The new password needs: At least 12 characters."
Flow 18's audit log and entry print the stored text as the row's result.

## Root cause

Inferred, not read: the register handler throws an `InvalidOperationException` from Wombat code naming Identity's error
code, and P1 keeps a Wombat-thrown `InvalidOperationException`'s message as the refusal's words (`AuditFailureText`).

## What to build

The handler refuses with the same words the page shows (the password rules' sentence `ResetUserPasswordCommand`
already stores: "The new password needs: …"), so the audit row reads as the refusal. Check the other Identity-backed
commands for the same shape. T399 (pages printing an exception's text) is the neighbouring case.

## Verification

- [ ] A too-short password at registration stores the page's refusal words in its audit row — an Application test and
  the replay's step 2.8 read by SQL.

## Related

T400 (flow 18, P1/F1), T399, `AuditFailureText`.

## Notes

- The other failed rows the replay read store refusals' own words (acts 3, 6 and A).
