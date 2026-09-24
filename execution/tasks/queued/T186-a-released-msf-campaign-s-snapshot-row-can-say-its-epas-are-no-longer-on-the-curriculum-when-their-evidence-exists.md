---
id: T186
title: A released MSF campaign's snapshot row can say its EPAs are 'no longer on the curriculum' when their evidence exists
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The wording is wrong, but no count is. Possibly only a dev-data artifact; confirm before fixing.
**Surfaced:** 2026-09-24, the T135 browser check (review 1's evidence snapshot on dev).

## Symptom

In review 1's snapshot, campaign 1's row reads "Also declared PAED-001, PAED-010, PAED-012, no longer on the trainee's
curriculum, so nothing was recorded for those". Campaign 2 says the same for PAED-002 and PAED-005. But recorded
`msf_cpsa` activities 1–5 exist for exactly those EPAs, now with `EpaId` stamped (T137), and PAED-001 is on the
trainee's curriculum. `MsfCampaignEpas` rows 1–5 have a null per-EPA stamp column; only row 6 (campaign 3) has one.

## What to find out, then build

Find out whether campaigns 1 and 2 were released by a code path that skipped the per-EPA stamp. If so, that is a real
defect in `ReleaseMsfCampaign`. If they predate the stamp, it is dev data, and W-007 says regenerate it. Either way, the
coverage sentence could be derived from the evidence rows themselves (the released campaign's `msf_cpsa` activities and
their `EpaId`), not from a stamp that can be missing.

## Verification

- [ ] The cause is recorded here.
- [ ] A released campaign's snapshot row names exactly the EPAs its evidence rows carry. Test.

## Related

T121, T137, T138.

## Cause found, 2026-09-24 (the T204 browser check)

`MsfCampaignEpas.RecordedOn` is NULL for campaigns 1 and 2, and for campaign 3's PAED-013. `DescribeCoverage` in
`StartCommitteeReview.cs` (around :305) reads a NULL as "no longer on the trainee's curriculum". Those releases predate
the per-EPA stamp, so it is most likely scenario data. But the sentence gives a reason it cannot know. Derive coverage
from the released campaign's `msf_cpsa` rows and their `EpaId`, and say "not recorded" rather than guess why.

## Widened 2026-09-24 (the T168 browser check)

T168's `GetMsfCoverageForTraineeQuery` has the same dependency on `MsfCampaignEpas.RecordedOn`. On dev it under-counts
PAED-001, 002, 005, 010 and 012, whose recorded `msf_cpsa` evidence (activities 1–5) exists. Fix both readers the
same way: a campaign covers an EPA when the released campaign's `msf_cpsa` rows carry that `EpaId` (T137 stamps it).
Then retire or backfill `RecordedOn`, W-007. Also re-check T166's 390px layout fix in the browser here.

