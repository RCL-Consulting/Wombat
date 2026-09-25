---
id: T282
title: The mail worker logs every recipient's address, which breaks the no-addresses-in-logs rule and makes MSF respondents linkable
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T282 — The mail worker logs every recipient's address, which breaks the no-addresses-in-logs rule and makes MSF respondents linkable

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Logs outlive anonymisation. An MSF respondent's address in the log, beside a campaign id,
undoes T207.
**Surfaced:** 2026-09-25, the T251 review. ARCHITECTURE.md says no email addresses go in logs.

## Symptom

`EmailWorker` and `MailKitEmailSender` log "Email to {To} …" on send, retry and drop, and `QueuedEmailSender` logs the
recipient at Debug. With the campaign tag in the same line, a respondent is linkable to a campaign for as long as logs
are kept.

## What to build

Log a message's tags and a stable, non-reversible reference (the tags already carry `campaign:N` and an invitation
key), never the address. Grep every logger call in `src` for `To`, `Email` and `Address` placeholders, and fix each.
Add an architecture or unit test that fails if a log template names an address placeholder.

## Verification

- [x] No log line carries a recipient address. A test over the templates, and a grep of a run's log on dev.

## Related

T251, T207, T205.

---

## As built — 2026-09-25 (`d4a3847`)

No log line carries a recipient's address. The mail worker and senders log each message's tags and a non-reversible
reference. `EmailLog.Redact` takes every form of an address (its IDN forms, its domain, and a standalone local part)
out of exception text. An architecture test judges every argument of every log call in `src`, including
`[LoggerMessage]` calls and `Define` delegates.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`): the app ran with Debug email logging.
- **Queued mail:** 12 mails, each with its own reference and one outcome.
- **The address grep** over 70,429 log lines found 0 addresses.
- **A refusing SMTP server:** retries at 2 s and 4 s, then "failed after 3 attempts", with `5.1.1 <[recipient]>`
  redacted. The campaign page counted the undelivered link, and Resend carried a new reference.
- **Not run:** the password-reset mail (reset is not wired yet).

**Filed from the review:** the no-SMTP fallback logs whole mails, so check before the deploy (noted on [T157]). The
remaining address leaks in exception text are noted on [T286].
