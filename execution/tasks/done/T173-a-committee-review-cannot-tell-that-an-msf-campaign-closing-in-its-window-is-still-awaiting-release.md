---
id: T173
title: A committee review cannot tell that an MSF campaign closing in its window is still awaiting release
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T173 — A committee review cannot tell that an MSF campaign closing in its window is still awaiting release

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. No evidence is wrong; a panel is simply not told that some is pending.
**Surfaced:** 2026-09-24, T138's review. It is a consequence of the released-only rule T138 adopted.

## Symptom

Since [T138] a committee's evidence snapshot holds only `Released` MSF campaigns, windowed by the day each closed.
A campaign that closed inside the window but is still `UnderReview` when the chair starts the review reaches **no**
snapshot. It is left out of this one because it is unreleased. It is left out of the next because its close date falls
in this window, and its per-EPA evidence is dated the same day. Before T138 the panel at least saw a "State:
UnderReview … to be recorded on release" row.

## What to build

A live notice on `ReviewDetail`, not frozen and not evidence: "N MSF campaigns that closed in this window are not yet
released". A count only, with no respondent data, scoped as the snapshot is. Alternatively, or as well, warn the chair
at Start.

## Verification

- [x] A review whose window holds an `UnderReview` campaign shows the count; one without shows nothing. bUnit or
      handler test.

## Related

[T138], [T121], [T131] (the agenda may subsume this).

---

## As built — 2026-09-24

`CountMsfCampaignsOutsideSnapshotQuery` returns two counts, both scoped as the snapshot is:
- **AwaitingRelease:** campaigns that closed in the window (the `ClosedOn` UTC date, as T138 windows them) and are
  still UnderReview.
- **ReleasedAfterStart:** after Start, window campaigns released but not named by this review's own evidence items.

`ReviewDetail` states each count in its own sentence and reloads the counts after every action. That reload has its
own error handling (`b0cb335`), so a failed count never sits beside the action's success. While the review is
Scheduled, a note beside Start review says why. Postgres tests cover the UTC edges and the single GROUP BY query.

**Recorded decision:** an External panel member from another institution learns the two counts, because the count is
scoped as the snapshot is. Counts only, no respondent data.

Browser on dev, 2026-09-24 (master `b0cb335`, the batch-C check): 
- Review 1 showed no notice while every campaign was released before Start.
- After campaign 4 was opened and closed (left UnderReview), the page reads "1 MSF campaign that closed in this window
  is not yet released." and "The snapshot holds only the campaigns released before the review started, so it is not in
  it."
