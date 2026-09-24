---
id: T206
title: MSF: the invitation expiry reminder can never send, and no product action withdraws a campaign
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A reminder that cannot fire is dead code presented as a feature, and T202's
anonymise-on-withdraw runs only in tests.
**Surfaced:** 2026-09-24, the T202 review (out-of-scope findings 2 to 4).

## Symptom

1. `MsfInvitationExpiryReminderJob.cs:40-51` needs `ExpiresOn` (ClosesOn + 7, from `AddMsfInvitation.cs:75`) to fall
   within 2 days on an Open campaign. That means today is at least ClosesOn + 5, but `MsfCampaignAutoCloseJob.cs:32`
   has already closed the campaign on ClosesOn + 1. Its test seeds `ExpiresOn` equal to ClosesOn, which the product never
   writes. `MsfExpiryReminderEmail` also names that unreachable expiry, says "a colleague", and names no trainee or
   campaign.
2. `WithdrawMsfCampaignCommand` has no caller in Web or Api.
3. An invitee added from another tab while an open is running is never mailed: `AddMsfInvitation.cs:78` does not touch
   the campaign row's concurrency token, and there is no resend.

## What to build

Key the reminder on ClosesOn (or set `ExpiresOn` = ClosesOn), and word it like T202's invitation. Add a Withdraw action
for a Coordinator on a draft or open campaign, with a confirmation. Make adding an invitee bump the campaign's token,
or have Open re-check the invitation set at save.

## Verification

- [ ] The reminder fires two days before ClosesOn on an open campaign. Job test with product-shaped data.
- [ ] A Coordinator can withdraw a campaign from the page, and its invitations are anonymised. bUnit and handler test.
- [ ] An invitee added during an open is either mailed or refused. Test.

## Related

T202, T132, T184.
