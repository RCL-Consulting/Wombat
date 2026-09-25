---
id: T225
title: MSF campaign pages polish: a quick template on an existing campaign, group keys in the report and PDF, and an Edit link in every state
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T225 — MSF campaign pages polish: a quick template on an existing campaign, group keys in the report and PDF, and an Edit link in every state

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Wording and stale controls.
**Surfaced:** 2026-09-25, the T217 implementation and review.

## Symptom

- The campaign page shows the "Quick template" card on a campaign that already exists.
- The report's headings and the PDF print respondent-group keys, not their labels.
- The campaign list's row link reads "Edit" whatever the campaign's state.
- `CloseMsfCampaign.CampaignChanged` still says "If the campaigns list still shows it as open, close it again." T217
  fixed the same wording for open and withdraw.
- Open is enabled on a draft with no invitees. The page says "Nobody has been invited yet." above it and the handler's
  refusal names the fix; a disabled button with a visible reason (the T107 pattern) would be better.

## Verification

- [ ] Each item above, with bUnit tests, and one browser pass over a draft, an open and a released campaign.

## Related

T217, T199, T107.
