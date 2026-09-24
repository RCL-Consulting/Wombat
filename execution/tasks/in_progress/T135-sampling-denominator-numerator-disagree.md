---
id: T135
title: "The sampling denominator and numerator disagree about what counts as a rating"
status: in_progress
priority: P2
owner: agent
depends_on: []
created: 2026-09-20
started: 2026-09-24
---

# T135 — Two ways `TotalRatedActivities` can be wrong while `EvidenceComplete` says it is not

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Neither is visible on the seed corpus today, and both are the same shape as the
defect [T134] just removed: a number a committee reads, wrong in a way the report asserts it is not.
**Surfaced:** 2026-09-20, in T134's design pass. Filed as one task rather than two because they are
one invariant, in one method, and would be verified together — the same convention as [T106] and
[T114]. The design panel recommended two; this is the deviation and the reason for it.

## The invariant

`GetSamplingConcentrationWarnings` reports `WithheldRatedActivities`, and `EvidenceComplete` is
`WithheldRatedActivities == 0`. The page renders that as the difference between *"we looked and it is
clean"* and *"we could not look"*. For it to mean anything, **every rated row in the window must be
either in the arithmetic or in the withheld count.** Two paths break that.

## Defect 1 — a draft counts as a rated observation

The window query filters on `SubjectUserId` and the `ObservedOn` bounds and **nothing else**. There is
no `CurrentState` predicate, and `TryParseRating` reads only `epa_id` and `assessor_user_id` — it never
looks at a rating value. So an activity that was created, had an EPA and an assessor named, and was
never submitted is counted as rated evidence a committee can sample.

T134 widened the rated set from four legacy keys to every rated type, which makes this reachable for
every v11.1 trainee where before it was reachable only for the four generic tools.

**Interacts with [T127]** — every failed Submit leaves an orphan draft behind, and the next attempt
makes another. Those orphans now land in a committee statistic.

## Defect 2 — an unparseable row is in neither column

A row that passes the rated gate and is readable, but whose `DataJson` fails `TryParseRating`, is
inside `activities.Count` and therefore **outside** `withheldRatedActivities` — while also being
outside `ratings.Count`, which is what `TotalRatedActivities` reports. It is silently in neither
column, and `EvidenceComplete` still reads true.

All four CPSA WBA seeds use the literal `epa_id` / `assessor_user_id` keys, so the corpus was safe when
this was filed. A builder-made type need not: nothing declares where an activity's EPA or assessor live,
which is the same class of gap [T126] closed for the rating and that [T122] is queued to close for the
tool.

**Updated 2026-09-21 by [T121]: the corpus is no longer safe, and the answer is not obvious.** `msf_cpsa`
declares `rated_level_field`, so it is rated, and it deliberately carries **no** `assessor_user_id` — an
MSF asserts a level but names no observing assessor, and calling the releasing reviewer one would be a
lie on a clinician-facing chart. Every released MSF evidence row therefore takes defect 2's path, every
time, on real data.

What that means for the fix matters: for MSF the *outcome* is right and only the mechanism is accidental.
An MSF has no assessor to concentrate, so leaving it out of an assessor-concentration numerator is
correct — folding it into `WithheldRatedActivities` would be wrong, because nothing was withheld. So the
third column this task proposes has to distinguish "unreadable" from "rated but not assessor-attributed",
rather than lumping them. The same distinction will be needed for [T120]'s three unrated instruments.

## What to build

1. A state predicate on the window query, and a decision recorded about which states count. "Completed
   only" is the obvious answer and it is still a decision — `procedure_log` and `journal_club` declare
   terminal initial states, so "completed" is not a synonym for "finished being edited".
2. Rows that fail `TryParseRating` counted somewhere a reader can see. Either fold them into
   `WithheldRatedActivities` (they are evidence the report could not use, which is what that number
   means) or give them their own count. Do not leave them in neither.

## Verification

- [ ] A draft in the window is not counted as rated evidence — handler test, would fail today
- [ ] A rated readable row with unreadable `DataJson` moves the report off `EvidenceComplete` —
      handler test, would fail today
- [ ] `TotalRatedActivities + WithheldRatedActivities` accounts for every rated row in the window —
      a property asserted directly, since it is the invariant both defects break
- [ ] Every existing sampling test stays green **without being edited** — the fixture is the evidence
      legacy behaviour is unchanged, as it was for T134
- [ ] Full suite green, no `--no-build`

## Related

Found in [T134]'s design pass. [T127] (orphan drafts) supplies defect 1's volume. [T122] would let
defect 2 be closed properly by declaring where an EPA lives rather than reading a literal key.

> **Update 2026-09-23 from [T122].** T122 declared which INSTRUMENT a type is (`ActivityType.WbaToolKey`), not where
> an activity's EPA lives. It needed no such declaration: its gate reads the EPA through the pinned credit rules'
> `epa_field` (`CreditTargetResolver`), and the picker through `CreditRuleFields.ResolveCreditedEpaFieldKeys`. That is
> the declaration defect 2 can read instead of the literal `epa_id`, for any type that credits. A type that credits
> nothing still has no declared EPA field.

## Notes

- **Observed 2026-09-20:** the window query carries no `CurrentState` predicate and `TryParseRating`
  reads two literal keys and no rating value. Both read directly from the source.
- **Not verified:** whether any draft or unparseable row exists in the dev or production data today.
  The defect is structural either way, and T134 widened who it can reach.
- **Observed 2026-09-21:** dev now holds three released `msf_cpsa` activities, each of which is a live
  instance of defect 2's path. They are readable by the committee, so `EvidenceComplete` still reads
  true; the visible consequence today is only that they are absent from `TotalRatedActivities`.

## Update 2026-09-24 — EPA-stream survey

**[T150] is folded in.** It is the same query (`GetSamplingConcentrationWarnings`) with the same verification, and T150
closes with this task. **[T106] item 9 comes with it**: the trajectory also charts rows in any state
(`GetEpaTrajectoryForTraineeQuery.cs:127-133` has no state predicate). Inferred: that is reachable today by a row an
assessor rated and then declined, because `declined` is a non-terminal dead end in the CPSA workflows.

**Item 1, which states count: the recommended decision.** Record it as a D-number when built. An activity counts as
sampled evidence when its `CurrentState` is **a terminal state of its pinned workflow**, the point where credit fires
(`ActivityService.cs:340`). That is `completed` for 11 of the 12 rated seeds and `recorded` for the twelfth,
`msf_cpsa`. Types with no workflow fall back to the literal `completed`. The test fixtures carry no `WorkflowJson`
(`SamplingConcentrationWarningsTests.cs:539-554`), so the existing tests stay green unedited. Drafts, `requested`,
`declined` and `cancelled` are out; in every CPSA WBA seed the last two are non-terminal dead ends, which this file
missed.
*Rejected:* literal `completed`, which drops `msf_cpsa`. Also rejected: "or carry a rating" (T150's wording), which
re-admits a rated-then-declined row. And credited-only, because a below-minimum rating is still evidence.

**Item 2: two new counts, not one.**

- `UnreadableRatedActivities` counts a row whose declared EPA or assessor field cannot be read. It sets
  `EvidenceComplete` false and gets its own sentence on the page. `ReviewDetail.razor:24-50` blames a gap on read
  scope, so folding these rows into `WithheldRatedActivities` would make that sentence false.
- A separate count covers rated rows with no assessor attribution: `msf_cpsa`, whose schema declares no assessor field,
  and rows with no rating value (D10 makes the MSF ordinal optional). It is reported and **does not** affect
  `EvidenceComplete`.
- To tell them apart, check whether the pinned schema declares the field. No new DSL property now.
- Read the EPA through the credit rules' `epa_field` (`CreditRuleFields.ResolveCreditedEpaFieldKeys`,
  `CreditRuleFields.cs:37`), with the literal `epa_id` as fallback. [T137]'s stamped `Activity.EpaId` replaces this
  once it lands.

**From T150:**

- "The assessor" is whoever the `field:` rule on the rated field's effective `editable_by` names, that is, whoever may
  write the rating. If that names none, fall back to every nominee field. Compare ids exactly: no `Trim`
  (`GetSamplingConcentrationWarnings.cs:317`).
- The trajectory reads the declared `rated_level_field`, not the literal `overall`/`overall_level`
  (`GetEpaTrajectoryForTraineeQuery.cs:476-481`), and gets the same state predicate.

**The committee snapshot keeps every state.** `StartCommitteeReview` labels each line with its state
(`StartCommitteeReview.cs:98`), so it keeps showing all of them. The handler comment claiming the two windows match
(`GetSamplingConcentrationWarnings.cs:105-109`) is wrong and gets corrected.

**Stale above:** "T120's three unrated instruments". [T120] shipped CCA, RCA and chart-stimulated recall as rated, each
carrying `assessor_user_id`. `reflective_exercise_cpsa` declares no `rated_level_field`, so it never enters the gate.

**Verification, added:**

- [ ] A draft, a cancelled and a declined row in the window move neither the rated count nor the distinct-assessor
      count. Handler tests.
- [ ] An `msf_cpsa` row is counted as unattributed and leaves `EvidenceComplete` true. Handler test.
- [ ] Total + Withheld + Unreadable + Unattributed equals the rated rows in the window in a qualifying state.
      Property test.
- [ ] A type whose assessor field has another key is counted, on the sampling report and on the trajectory. Tests.
- [ ] The trajectory plots no draft or declined row, and does plot a rated builder type whose rated field is not
      `overall_level`. Query tests.
- [ ] Browser, on dev: a committee review for a trainee with a released MSF.

---

## As built — 2026-09-24 (with [T150] and [T106] item 9)

**D44, decided here** (recorded in `EPA-PROGRAMME.md` § 3D). The committee sampling report and the entrustment
trajectory count an activity only when its state is a terminal state of its **pinned** workflow, which is where credit
fires. That is `completed` for the assessor-rated seeds and `recorded` for `msf_cpsa`; a type with no workflow falls
back to `completed`. Drafts and requests are out. So are `declined` and `cancelled`, wherever the pinned workflow makes
them non-terminal dead ends, as every current seed does.

What is read from each row, all from the pinned version:
- **EPA:** the stamped `Activity.EpaId` ([T137]). A null stamp, or an EPA that no longer exists, makes the row
  unreadable.
- **Rating:** `rated_level_field`, else the type's current one. The version must declare the field, or its rows are
  unreadable.
- **Assessor:** the fields named by the `field:` rule on the rating's `editable_by`, the field's own rule before its
  section's. Else the `field:` actors of the transitions into a terminal state. Else every nominee field, when a role
  or scope writes the rating. Else nobody. Ids are compared exactly.

Every evidence row lands in exactly one of four counts. The test `EveryEvidenceRowIsInExactlyOneCount` checks this
over 8 seeded random mixes:
- **Attributed:** counted in the figures.
- **Withheld:** the caller may not read it, and its version names an assessor.
- **Unreadable:** a declared field is missing or malformed, or empty where every transition into the row's state
  required it.
- **Not attributed:** the version names nobody for the rating (MSF), or the rating or assessor was left empty where the
  form allows it.

Withheld and unreadable make the report incomplete (`EvidenceComplete`). Not attributed does not.

*Rejected:*
- literal `completed`, which drops `msf_cpsa`;
- "completed or carries a rating", which lets a rated-then-declined row back in;
- credited-only, since a below-minimum rating is still evidence;
- folding unreadable into withheld, since the page blames withheld on read scope;
- counting a withheld MSF as withheld, which would make the banner false;
- the first nominee as the assessor when the trainee writes their own rating, which pads the count.

**Code**
- `RatedEvidence.cs` builds one profile per pinned (type, version) and is shared by both readers. The old literal-key
  parser is gone.
- `SamplingConcentrationReportDto` gains `UnreadableRatedActivities` and `UnattributedRatedActivities`.
- `ReviewDetail.razor` states each count in its own true sentence, and "Ask a panel member…" appears only for withheld
  rows.
- The snapshot in `StartCommitteeReview` still lists every state, and its comment is corrected: the report is live, the
  snapshot is frozen at Start.

**Evidence.** Two rounds, 45 mutants, all killed. At the merge the EPA source was switched to the stamped `EpaId`; that
switch was mutation-checked too (5 tests fail without it). Suites on master: Domain 398, Application 1022,
Infrastructure 658, Architecture 28, Web 391, Integration 28.
