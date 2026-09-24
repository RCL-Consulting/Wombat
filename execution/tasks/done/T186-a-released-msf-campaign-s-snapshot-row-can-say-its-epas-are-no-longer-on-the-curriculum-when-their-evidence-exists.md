---
id: T186
title: A released MSF campaign's snapshot row can say its EPAs are 'no longer on the curriculum' when their evidence exists
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
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

- [x] The cause is recorded here.
- [x] A released campaign's snapshot row names exactly the EPAs its evidence rows carry. Test.

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

---

## As built — 2026-09-24

**Cause.** Campaigns 1 and 2 predate the T121 per-EPA stamp, so their `MsfCampaignEpas.RecordedOn` is null although
their evidence rows exist. It was not a release bug. Every reader trusted the stamp.

**Fix.** One reader, `MsfCampaignCoverage.RecordedEpasAsync`. A released campaign covers an EPA when its **finished**
evidence rows carry that `EpaId`: rows of the given evidence type (a parameter, for T164) whose `DataJson.campaign_id`
names the campaign and whose state is terminal in their pinned workflow.
- **Used by:** the committee snapshot sentence, T168's coverage card, the campaign report and the portfolio PDF.
- **Wording.** The snapshot says "Evidence recorded for …" and "Declared but not recorded: …", and never guesses a
  reason.
- **The stamp column.** Nothing reads `MsfCampaignEpa.RecordedOn` any more. The release still writes it, and the column
  stays until a later migration drops it.

**Browser on dev (scripted Chrome, `26bc740`):**
- The trainee's "This period" line reads "6 of 15 EPAs covered by a released campaign that closed this semester".
  PAED-001, 002, 005, 007, 010 and 012 are covered; PAED-003 (unreleased) and PAED-013 (declared, no evidence) are not.
- Review 2's MSF card reads "6 of 15 EPAs covered so far", naming each campaign. Its frozen snapshot keeps the old
  wording, because it was frozen before the fix.
- Campaign report 1 lists PAED-001, 010 and 012 with no suffix. Report 3 marks PAED-013 "— not recorded". No report says
  "no longer on the trainee".
- T166's 390px fix holds on both pages: the page scrollWidth is 390, and the Entrustment table scrolls in its own
  container.

