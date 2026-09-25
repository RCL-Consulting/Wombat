---
id: T246
title: Closing an MSF campaign that is already under review succeeds again and moves ClosedOn, which decides its semester and evidence date
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-25
---

# T246 — Closing an MSF campaign that is already under review succeeds again and moves ClosedOn, which decides its semester and evidence date

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A stale tab can move a campaign into another semester and committee window.
**Surfaced:** 2026-09-25, the T225 review. It predates T225.

## Symptom

`MsfCampaign.Close` accepts a campaign that is already under review and sets `ClosedOn` again. A report tab loaded while
the campaign was open, with the campaign then closed elsewhere (another tab or the auto-close job), can press Close. It
succeeds, reads "Campaign closed and anonymised for review.", and moves `ClosedOn` to that day. `ClosedOn` decides:
- the coverage semester (`GetMsfCoverageForTrainee`);
- the committee window (`MsfCampaignReviewWindow`);
- the evidence date (`ReleaseMsfCampaign`).

Callers are `CloseMsfCampaign` and `MsfCampaignAutoCloseJob`, and the job selects open campaigns only.
`MsfCampaignOpenCloseTests.Close_LeavesAnAlreadyAnonymisedInvitationAsItWas` says "Close is legal from UnderReview
too", which was deliberate at the time.

## What to build

`Close` refuses anything but Open ("Only open campaigns can be closed.") before any change. The report page's refusal
re-reads the campaign (T217's pattern). Update the Domain test deliberately.

## Verification

- [ ] A second close is refused and `ClosedOn` is unchanged. Domain and handler tests (the audit trap).
- [ ] Browser: two tabs; closing in the second is refused.

## Related

T225, T217, T168, T186.
