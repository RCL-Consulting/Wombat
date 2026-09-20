---
id: T081
title: "Graduate committee decision category (F-5-1)"
status: done
priority: P3
---
# T081 — Graduate committee decision category (F-5-1)

**Status:** ✅ Shipped 2026-06-01 (Opus). Final Act 5 finding.

## Problem (F-5-1)

`CommitteeDecisionCategory` had no terminal graduation outcome
(`SatisfactoryProgress / SatisfactoryWithObservations / InadequateProgressAdditionalTraining /
InadequateProgressRepeat / ReleaseFromTraining / OutcomeDeferred`). The Act 5 graduation review had to
be recorded as `SatisfactoryProgress` — a "programme complete" outcome had no first-class representation.

## Fix

- Added `Graduate = 7` to `CommitteeDecisionCategory` (terminal "programme complete" outcome).
- Added its PDF display label in `CommitteeSectionComponent.FormatCategory`
  ("Graduate (programme complete)").
- No other changes needed: the `ReviewDetail` decision + appeal-replacement dropdowns are
  `Enum.GetValues`-driven (auto-include it); the category is stored as an int (no migration); no
  handler branches on specific categories.

This complements T080: the committee records `Graduate` as the review outcome, and the trainee
profile is then archived via the graduation lifecycle (Mark complete → role removal).

## Tests

`CommitteeReviewTests.Review_RecordsAndRatifies_GraduateDecision` (+1 domain): start → record a
`Graduate` decision → ratify → current decision category is `Graduate`. Domain 48→49. All suites green
(Infrastructure 10, Application 268, Architecture 19, Web 42); Integration not run (Docker).

## Note

Not browser-re-driven: the decision dropdown is reflection-over-the-enum, so the new value appears
mechanically, and the record→ratify path is covered by the domain test. Molefe's Act 5 review remains
recorded as `SatisfactoryProgress` (it is immutable post-ratification); a future Act 5 replay would use
`Graduate`.
