---
id: T269
title: A trainee who also coordinates, opening their own released MSF report, gets the coordinator's report page
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T269 — A trainee who also coordinates, opening their own released MSF report, gets the coordinator's report page

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Since the T246 review the actions card is read-only, but the subject should see the trainee's view.
**Surfaced:** 2026-09-25, the G2 browser check and the T248 review (finding 7).

## Symptom

`CanReadReportAsync` admits the subject once the report is released, so a Trainee plus Coordinator opening
`/msf/reports/{id}` for their own campaign gets `CampaignReport.razor`. That is the coordinator's page, with its actions
card and per-group detail.

## What to build

Send the subject to `/msf/my-reports/{id}`, the trainee's own view, from the coordinator's page. Check that no
teaching-context name (T164) reaches them on either page.

## Verification

- [ ] The subject lands on My MSF reports and sees no coordinator card. bUnit or handler test.

## Related

T224, T248, T164, T205.
