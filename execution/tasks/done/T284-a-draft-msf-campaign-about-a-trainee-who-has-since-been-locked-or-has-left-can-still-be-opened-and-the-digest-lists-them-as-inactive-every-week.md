---
id: T284
title: A draft MSF campaign about a trainee who has since been locked or has left can still be opened, and the digest lists them as inactive every week
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T284 — A draft MSF campaign about a trainee who has since been locked or has left can still be opened, and the digest lists them as inactive every week

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T268 review.

## Symptom

- **Opening a draft.** `OpenMsfCampaign`, `AddMsfInvitation` and `ResendMsfLinks` check only the campaign's scope, not
  whether the trainee is still current. Opening a draft therefore mails every respondent about someone who is locked,
  has graduated or has withdrawn. The remarks on `MayStartCampaignAboutAsync` say only a new campaign asks.
- **The digest.** `WeeklyCoordinatorDigestJob` builds its inactive list from `GetUsersInRoleAsync(Trainee)` and
  `ReadableAsync`, so a locked or withdrawn trainee who kept the role is listed as inactive every week.
- **The lock card.** The admin lock card on `UserDetail` says only that locking "prevents sign-in immediately". It
  should say what else a lock does now: no panel seat, not a current trainee, no reminders.

## Decision (adopted 2026-09-25; the operator may overrule)

Opening a draft is new work: it mails people. Open and Add refuse a subject who is not current, with the same refusal
create gives. Resend and the reminders of an already-open campaign carry on.

## Verification

- [x] Opening a draft about a locked trainee is refused. The digest leaves out non-current trainees. Tests.
- [x] The lock card names the effects. bUnit.

## Related

T268, T238, T117.

---

## As built — 2026-09-25 (`0ea7f56`)

Opening a draft is new work (the decision adopted in the task).
- **Open and Add** refuse a subject who is no longer current, as create does. Resend and the reminders of an already open
  campaign carry on.
- **The weekly digest** keeps only current trainees.
- **The admin lock card** names what a lock does.

Handler, job and bUnit tests.

Browser on dev (scripted Chrome, master `e22d58b`; `pg_dump -n public` first, at `recovery/pre-t283-t281-migrations.dump`):
- **The lock card** reads as designed.
- **With the trainee locked,** Add and Open on draft 27 were refused (role=alert, 0 mails), and the picker dropped them.
  An open campaign's Resend still sent.
- **After reactivation,** Open sent exactly one mail.

**Filed from the review:** the page still offers Add and Open before refusing them (noted on [T270]).
