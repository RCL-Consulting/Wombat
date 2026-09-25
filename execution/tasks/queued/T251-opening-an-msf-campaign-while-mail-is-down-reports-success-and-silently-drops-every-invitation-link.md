---
id: T251
title: Opening an MSF campaign while mail is down reports success and silently drops every invitation link
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T251 — Opening an MSF campaign while mail is down reports success and silently drops every invitation link

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The coordinator is told each respondent was emailed. Nobody was, and nothing on any page says so.
**Surfaced:** 2026-09-25, the G2 browser check (T225 step 4, campaign 13 on dev).

## Symptom

Stop the SMTP sink, add an invitee to a draft campaign, and press Open. The page reads "Campaign opened, and each
respondent has been emailed a link to respond." and the campaign is Open. The log then shows "Email to … (tags:
msf-invite, campaign:13) failed after 3 attempts. Message dropped."

## Cause (observed)

`OpenMsfCampaignCommandHandler` sends through `IEmailSender`, which is `QueuedEmailSender`: a hand-off to an in-process
channel that never throws. `EmailWorker` delivers later, retries three times, then drops the message. T184's
`InvitationsNotSent` refusal can therefore never fire, and a dropped link is a respondent who can never answer. Every
other mail has the same property (account invitations, nudges, digests), but a lost MSF link costs a respondent.

## What to build

- **Recommended:** record delivery per invitation. `EmailWorker` reports each message's outcome, keyed by the tags it
  already carries (`msf-invite`, `campaign:N`, and an invitation key to add), onto `MsfInvitation` (`SentOn`,
  `DeliveryFailedOn`).
- The campaign page shows "N links not delivered" while the campaign is open, with a Resend for those invitees. Resend
  issues a new link, as a reminder does (T214).
- Remove or reword the success message: "Campaign opened; links are being sent." Keep `InvitationsNotSent` only for a
  hand-off that fails.
- Account invitations should get the same outcome record: file that if it is not done here.
- *Rejected:* sending MSF mail synchronously in the request. It holds the circuit on SMTP, and a partial failure half-way
  through the list leaves some respondents mailed and the campaign in no clear state.

## Verification

- [ ] With delivery failing, the campaign page names the undelivered count and Resend delivers once mail is back. Job
      and handler tests with a failing sender; browser with the sink stopped, then started.

## Related

T225, T184, T202, T214, T205.
