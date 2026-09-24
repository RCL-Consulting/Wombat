---
id: T168
title: No surface shows, per EPA and period, whether an MSF covered it
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T168 — Nothing shows whether an EPA was covered by an MSF this period

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low-Medium. MSF is required on all 15 EPAs (D37), is "tracked in its own right" (D8), and is run per period
across many EPAs (D9). No page answers "EPA 7: no MSF this semester". The evidence exists only as rows on the activity
list and lines in the committee snapshot.
**Surfaced:** 2026-09-24, the EPA-stream survey, gap "MSF coverage per EPA and period". [T137] covers only telling the
per-EPA rows apart on the activity list.

## Symptom

Observed at `431e69e`:

- `msf_cpsa` ships `counts_for: []` (D8; `ReleaseMsfCampaign.cs:135-141`), so a released campaign moves no
  `CurriculumItemProgress` row. This is correct, and it is permanent per pinned version.
- The only progress read model reads `CurriculumItemProgress` alone (`TraineeQuotaProgress.cs:165-196`).
  `MyProgress.razor`, `TraineeDashboard.razor` and `CurriculumCoverage.cs` never mention MSF (case-insensitive grep).
- D9 (`EPA-PROGRAMME.md` § 3A-ii) fixes the meaning: "MSF completed for EPA 7" means "a campaign covering EPA 7 was
  released this period", and "say that wherever the phrase is printed". `MsfCampaignEpa` is the join that says which
  EPAs a campaign covered.

## What to build

A per-EPA, per-period MSF coverage read: for each curriculum item, whether a **released** campaign covering its EPA
closed in the period ([T130]'s `AcademicPeriod`), and when. Build it from `MsfCampaignEpa` and campaign release, not from
`counts_for`, and do not route it through `CurriculumItemProgress`. Show it:

- on the trainee's progress page, beside each EPA's quota line, worded as D9 requires;
- on the staff coverage view, as "n of m trainees covered this period";
- in the committee's per-EPA evidence group ([T167]).

Draft, open and withdrawn campaigns do not count ([T138] makes the same cut for the snapshot). Confirm from Annexure B
whether the requirement is one campaign per period or some other cadence before printing a target. Until then, show
coverage, not a shortfall.

## Verification

- [x] A released campaign covering PAED-001 and PAED-007 in semester 1 shows both as covered in semester 1 and neither in
      semester 2; a withdrawn campaign covers nothing. Query tests.
- [x] The progress page shows the coverage line in D9's wording. bUnit test, and in the browser on dev with a released
      campaign.
- [x] The query is scoped like the progress reader ([T113]'s ladder). Test.
- [x] Full suite green, no `--no-build`.

## Related

D8, D9, D37, [T121] (MSF evidence rows), [T130] (periods), [T137], [T138], [T167], [T164] (learner feedback reuses
MSF).

---

## As built — 2026-09-24

`GetMsfCoverageForTraineeQuery` works per EPA and per semester (T130's calendar). An EPA counts as covered in a
semester when a **released** campaign covering it closed in that semester (by `ClosedOn`, T138).
- MSF counts toward no target (D8).
- An ended semester's gap is not worded as final: "no campaign covering this EPA that closed in the semester has been
  released".
- The review card's "Read live" sentence follows the review's state.

**Recorded readings:**
- Coverage is its own card, not lines inside the evidence groups.
- A campaign's semester is set by the day it closed, where D9's text says "released this period".

**Browser on dev (scripted Chrome):**
- **Wording.** It is right for covered, running and ended semesters, and unreleased campaigns 4 and 5 are excluded.
  PAED-007 reads "covered by a released campaign that closed on 21 September 2026".
- **Coverage is wrong on dev:** PAED-001, 002, 005, 010 and 012 read uncovered, although recorded `msf_cpsa` evidence
  exists for them. The query reads `MsfCampaignEpas.RecordedOn`, which campaigns 1 and 2 lack because they predate the
  T121 stamp. [T186] fixes both readers by deriving coverage from the evidence rows.

**Filed:** [T210] (the staff "n of m trainees covered" view, the T168 review's finding 5).

