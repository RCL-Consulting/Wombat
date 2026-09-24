---
id: T138
title: The committee evidence snapshot includes draft, open and withdrawn MSF campaigns
status: done
priority: P3
owner: agent
model: sonnet
depends_on: []
created: 2026-09-21
started: 2026-09-24
completed: 2026-09-24
---

# T138 — A panel is shown feedback that was never released, and feedback that was withdrawn

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low-to-Medium. It shows a committee a row for evidence the trainee has never seen and, in
the withdrawn case, for evidence somebody deliberately retracted. No respondent data leaks — the summary
is state, response count and close date — but the row is presented as evidence.
**Surfaced:** 2026-09-21, reading `StartCommitteeReview` while wiring [T121]'s per-EPA evidence.

## Symptom

`StartCommitteeReview.BuildEvidenceSnapshotAsync` selects MSF campaigns by subject and by `ClosesOn`
falling in the review window, and applies **no state filter at all**. A campaign still in `Draft`,
still `Open`, sitting in `UnderReview`, or explicitly `Withdrawn` is snapshotted into the panel's
evidence list exactly like a released one. The summary does print the state, so a careful reader can
see it — but it is in the list.

Contrast the two places that get it right: `PortfolioPdfService` filters
`State == MsfCampaignState.Released`, and `ListMsfCampaignsForTraineeQuery` does the same.

## Root cause

The query predates the state machine being load-bearing, and nothing ever compared the three MSF
readers against each other.

## What to build

Decide which states belong in a panel's evidence and apply it in one place. The obvious answer is
`Released` alone, matching the portfolio and the trainee's own view; a defensible alternative is
`Released` plus `UnderReview` with the state made prominent, on the grounds that a panel meeting while
a campaign is under review may want to know it exists. `Withdrawn` is clearly out either way.

Worth doing at the same time: the activity side of the same method has no state filter either, which is
correct (a declined WBA is evidence about the trainee's progress), so the asymmetry should be stated in
a comment rather than left to be "fixed" later.

## Verification

- [x] A withdrawn campaign closing inside the review window does not appear in the snapshot — checked
      by a `CommitteeDecisionHandlersTests` case (`StartReview_LeavesOutAnMsfCampaignThatWasNeverReleased`, a theory over
      Draft, Open, Closed, UnderReview and Withdrawn)
- [x] A released one still does, with its [T121] coverage sentence intact — checked by the existing
      `StartReview_MsfEvidenceNamesTheEpasItStandsBehind`, plus `StartReview_IncludesAReleasedMsfCampaignClosingInTheWindow`
- [x] Full suite green — `dotnet test` per project, no `--no-build` (on master after the merge; see the commit)

## Related

[T121] added the coverage sentence to the same evidence row. [T101] finding E is the neighbouring
committee-authorization work.

## Notes

- **Observed:** the snapshot is a point-in-time copy, so fixing the query changes only future reviews.

---

## As built — 2026-09-24

- **Decision (a): Released only**, written positively (`campaign.State == MsfCampaignState.Released`), the rule
  `PortfolioPdfService` and `ListMsfCampaignsForTraineeQuery` already use. The activity side stays unfiltered; a comment
  says why (a declined WBA is evidence of progress, and an MSF is evidence only once released).
- **Found by the review, fixed: the parent row and its per-EPA evidence could land in different reviews.** The campaign
  was windowed by `ClosesOn` (scheduled), while its per-EPA activities are dated `ClosedOn` (actual,
  `ReleaseMsfCampaign.EvidenceCompleteOn`). Every auto-closed campaign scheduled for a window's last day was split,
  because `MsfCampaignAutoCloseJob` closes the day after. It is now windowed by the UTC date of `ClosedOn`, and a note on
  `EvidenceCompleteOn` says the two must move together.
- `DescribeCoverage`'s non-released branches are deleted. `ReviewDetail`'s empty snapshot says "Starting the review
  captures…" only while `Scheduled`; once started it says "The review window held no evidence when this review started."
- Tests: Application 895 → 906, Web 288 → 293. Four mutants (the window, the end bound, the filter, the Razor branch)
  were all caught.
- **Filed:** [T173], the live notice for a campaign closing in the window but still awaiting release.

