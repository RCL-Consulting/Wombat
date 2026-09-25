---
id: T225
title: MSF campaign pages polish: a quick template on an existing campaign, group keys in the report and PDF, and an Edit link in every state
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] Each item above, with bUnit tests, and one browser pass over a draft, an open and a released campaign.

## Related

T217, T199, T107.

---

## As built — 2026-09-25 (`133468f`)

- **Labels.** Campaign pages, the report headings and the PDF name respondent groups by label.
- **List links.** Each list row's link names what its state allows ("Manage", "Review and release", "View report").
- **Quick template** shows only when creating a campaign.
- **Open.** It waits for invitees: it is disabled with a visible reason and `aria-describedby`. Close, Release and Open
  keep the focus while they run.
- **Refusals.** A refused release keeps the report. Close's refusal wording matches T217's.

Browser on dev (scripted Chrome, master `ec58d2e`; `pg_dump` first, at `recovery/pre-g2-migrations.dump`):
- **List rows** follow the state table.
- **Campaign 13** had no Quick template card. Open was disabled with "Open campaign: add at least one invitee first…"
  and enabled after the first invitee.
- **Campaign 14** opened with three mails. Three responses came through the mailed links. Close, then Release, wrote MSF
  evidence activity 40.
- **Group headings** are labels on the reports, on My MSF reports and in the PDF.
- **Two tabs:** a stale Close was refused, the state re-read, and the focus kept.
- **390px and 1280px:** no page scroll.
- **Failed, not T225's own code:** step 4 expected T184's "invitations not sent" refusal with mail down. The campaign
  opened and reported success, and the links were dropped by the queued sender. Filed as [T251] (P2).

**Filed from the review:** [T246] (a second close moves `ClosedOn`), [T249] (the PDF counts suppressed groups).
