---
id: T134
title: "Committee sampling reports zero rated evidence for every v11.1 trainee"
status: done
priority: P1
owner: agent
depends_on: []
created: 2026-09-20
started: 2026-09-20
completed: 2026-09-20
---

# T134 — The sampling-concentration report counts only the four legacy tool keys

**Severity:** High. A committee ratifying an entrustment decision is shown "no rated evidence" for a
trainee who has it. **This is wrong on screen today**, on the CPSA v11.1 catalogue, which is the live
workstream.
**Surfaced:** 2026-09-20, by the completeness critic in T126's design pass. Nothing was looking for
it; it sits on the same "which activities are rated" seam and nobody had walked it.

## Symptom

`GetSamplingConcentrationWarnings` declares its own answer to "which activity types produce a
rating":

```csharp
private static readonly IReadOnlyDictionary<string, WbaSourceCategory> SourceByActivityKey =
    new Dictionary<string, WbaSourceCategory>(StringComparer.Ordinal)
    {
        ["mini_cex"] = ..., ["dops"] = ..., ["cbd"] = ..., ["acat"] = ...
    };
```

and uses it as the **SQL filter**:

```csharp
var ratedActivityKeys = SourceByActivityKey.Keys.ToArray();
... ratedActivityKeys.Contains(activity.ActivityType.Key)
```

`StringComparer.Ordinal` on exact keys. So `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`,
`direct_observation_cpsa` — the entire seeded v11.1 tool set — and every `*_paed` key fall outside
it. A v11.1 trainee's report returns `TotalRatedActivities: 0`, and the panel is told there is no
rated evidence to sample.

## Root cause — a third, disagreeing definition of "rated"

`GetEpaTrajectoryForTraineeQuery` answers the same question by **family prefix**, so
`mini_cex_cpsa` matches `mini_cex`. This file answers it by exact key. They disagree, and the
disagreement is invisible because each reads correctly on its own.

That makes **three** independent definitions of which activity types produce an entrustment rating:
this exact-key list, the trajectory's family-prefix list, and the credit rules' `minimum_level_field`.
[T126] added a fourth answer that is actually authoritative — the schema's `rated_level_field` — but
deliberately did not retire the other three.

## What to build

The narrow fix is to match by family prefix as the trajectory does. **Resist it.** A fourth
copy of the same matcher is how this defect was made.

Prefer: one shared resolver for "does this activity type produce an entrustment rating, and on what
ladder", built on [T126]'s `rated_level_field`, with the hard-coded lists retired as they are
replaced. `WbaSourceCategory` is a *classification* (direct observation / conversation / case
analysis) that the trajectory also derives from its list — so the shared thing has to answer both
"is it rated" and "what kind of evidence is it", or the two will drift again.

Note [T122] is queued to put `WbaToolKey` on `ActivityType`, and the trajectory query's own
KNOWN LIMITATION comment names exactly that as the real fix. **Sequence this with T122** rather than
inventing a fifth mechanism ahead of it.

## Verification

- [ ] A v11.1 trainee with completed `*_cpsa` activities gets a non-zero `TotalRatedActivities` —
      checked by a handler test seeded with CPSA tool keys, which would fail today
- [ ] A legacy-tool trainee's report is unchanged — checked by the existing tests staying green
- [ ] The number of independent "which types are rated" definitions went **down**, not up —
      checked by grep for the hard-coded lists
- [ ] Full suite green, no `--no-build`

## Related

Same seam as [T126], which made the authoritative answer available but retired nothing.
[T122] (`WbaToolKey`) is the structural fix the trajectory query's own comment asks for.
Contradicts [T126]'s "nothing is wrong on screen today" — that framing was about the trajectory and
did not survive contact with this file.

## Notes

- **Verified 2026-09-20** by reading the source: the dictionary is exact-key with
  `StringComparer.Ordinal`, and `ratedActivityKeys.Contains(...)` is the query predicate.
- **Not verified:** whether any committee review has actually been run against a v11.1 trainee on
  dev or production, i.e. whether anyone has *seen* the zero. The defect is structural either way.

---

## Outcome — 2026-09-20

### What was built

One shared classifier, `RatedActivityTypes` (`Features/Activities/Services/`, beside
`CreditRuleFields` — the house location for a pure Application-side resolver over Domain parsers).
It answers both halves of the question the old dictionary answered privately:

- **Is this type rated?** A **disjunction**: it declares a rating ([T126]'s `rated_level_field`, the
  authoritative answer) **or** its key matches a known tool family.
- **What evidence is it?** `WbaEvidenceSource { DirectObservation, Conversation, CaseAnalysis }`,
  whose `Label()` returns the three strings the trajectory chart already printed, verbatim.

### Why a disjunction rather than the declared pointer alone

The pointer alone is cleaner and would have been wrong today, for two independently sufficient
reasons — both verified, not argued:

1. **The four operator-built `*_paed` types can never carry a pointer.** They are in no seeder,
   `ActivityTypeSeedRefresher` skips anything it does not own, and [T133] blocks the only manual
   route. A declaration-only gate would have left a second population reading zero, in a change made
   to stop a population reading zero.
2. **It would have forced rewriting the test fixture.** `SeedActivityTypeAsync` creates types with
   `SchemaJson` null, so under a pointer-only gate *every existing sampling test* would report zero.
   Rewriting the fixture to make types rated by default would have satisfied this task's own
   verification — *"a legacy-tool trainee's report is unchanged, checked by the existing tests staying
   green"* — with tests that had been rewritten to be green. **The fixture is untouched.** That is
   what makes the existing suite evidence rather than a restatement.

The family arm is documented as interim, with its retirement condition named: [T133] makes the
pointer authorable, [T122]'s `WbaToolKey` replaces the body.

### The constraint that shaped the query

The denominator is counted in SQL over rows the caller may **not** read, before `WhereReadableBy` —
that is what lets the report distinguish a clean sample from one it only saw part of. The rated test
is not SQL-translatable, but it does not need to be: rated-ness resolves to a set of `ActivityType.Id`
first, and `ratedTypeIds.Contains(activity.ActivityTypeId)` is the predicate. `CountAsync`,
`WhereReadableBy` and the `withheldRatedActivities` subtraction are untouched. The
`Include(ActivityType)` is dropped — the source bucket is looked up by id, so the join reads nothing.

### One behavioural change, taken deliberately

A rated row whose family is unknown **keeps its row** instead of being skipped. It must: it is in the
denominator, and dropping it from the numerator would under-report `TotalRatedActivities` while
`EvidenceComplete` still read true — the same lie this task removes, moved somewhere harder to see.
Its own key becomes its source bucket, so two activities of one unfamiliar type count as one source
rather than none.

### Definitions of "which types are rated": 3 → 1 shared, plus the credit rules

`SourceByActivityKey` (exact-key, committee) is **deleted**. `SourceByActivityFamily` (trajectory) is
**moved**, not copied. `enum WbaSourceCategory` is deleted with both its members that grep proved dead
— `LongitudinalObservation` and `ProductEvaluation` were declared in June and referenced by nothing,
ever. The credit rules' `minimum_level_field` still answers its own question, which is a different one.

### Not done, deliberately

The trajectory query is gated on the **family arm alone** (`verdict.Category is not null`), not on
`verdict.IsRated`. That keeps it byte-identical to what charted before, because widening what charts
is a change a clinician sees and wants a browser check this session could not do. It is a one-line
flip and belongs with [T133]/[T122]. The KNOWN LIMITATION comment is rewritten to say so.

### Verification

- [x] A v11.1 trainee's report counts rated evidence — `CpsaToolKeys_AreCountedAsRatedEvidence`,
      **written first and confirmed returning 0 against the unfixed handler**
- [x] A legacy-tool trainee's report is unchanged — the 12 pre-existing sampling tests pass **with the
      fixture untouched**
- [x] The number of "which types are rated" definitions went **down** —
      `grep -rn "SourceByActivityKey\|SourceByActivityFamily" src/` returns **two hits, both in the
      one shared file** (its declaration and its use). Before: two separate maps in two handlers.
- [x] 13 unit tests over `Classify`: both arms, the `<family>_` prefix rule, a near-miss that must not
      match (`dopsomething`), the three labels as literal strings, an unparseable schema under a known
      family still rated, the six unrated seeds not rated
- [x] 5 new handler tests including the two the design panel showed a rival design got wrong: two
      ratings from one unfamiliar rated tool are **one** source, and an operator-built `*_paed` tool is
      still counted via the family arm
- [x] **Verified to fail against the unfixed code, three ways.** Gate reverted to the four literal
      keys → 5 fail. `continue` restored for an unknown family → 2 fail. Rated filter moved behind
      `WhereReadableBy` → the two withheld-evidence tests fail, which is the denominator constraint
      proving it is load-bearing.
- [x] `dotnet build Wombat.sln -c Release` — 0 warnings, 0 errors
- [x] Suites green, no `--no-build`: Domain **78**, Application **544**, Infrastructure **207**,
      Architecture **23**, Web **103** — **955 total**, up from 924

### Browser-verified 2026-09-20, against the dev database

Signed in as Administrator, opened `/committee/reviews/5` — trainee
`c74afb9d-2d5e-4536-a3ff-4a3c5a589094`, on the CPSA v11.1 catalogue. **The Sampling concentration
warnings panel renders**, reading:

> 1 rated observation from 1 distinct assessor in the review window.
> **PAED-001 — Providing paediatric emergency care to children** · 1 rating · 1 assessor · 1 source
> Fewer than three distinct assessors across this EPA's evidence.

That trainee's activities are `mini_cex_cpsa`, confirmed by opening one and seeing the six-rung
CPSA entrustment field. **Under the old exact-key gate `mini_cex_cpsa` matched nothing**, so
`TotalRatedActivities` was 0, `AnyWarning` was false, and `ReviewDetail.razor` rendered **no panel
at all**. The committee was shown nothing where it should have been shown a concentration warning.

### Still not verified

- **The withheld-evidence path** (`EvidenceComplete: false`) could **not** be reproduced on dev.
  Attempted 2026-09-20 as `vanrensburg@sun.wombat.local`, who is external to the trainee's institution
  (sun.wombat.local vs kgk) and sits on the panel: they see the identical report to a global
  Administrator, with no incomplete banner. Panel membership evidently admits the rows through
  `WhereReadableBy`, so the case needs activity scope stamps engineered for it and there is no UI
  to do that. It stays covered by `ExternalPanelMember_IsToldTheSampleIsIncompleteRatherThanClean`
  and `PartiallyWithheldEvidence_FlagsTheWarningItInventedAsIncomplete`, both of which this task
  verified fail when the rated filter is moved behind the readability filter.
- No review on dev exercises an unfamiliar rated tool, so the `SourceBucket` fallback is
  test-covered only.

---

## Amended 2026-09-20 — the disjunction is gone

**The gate is now `declaresRating` alone.** Everything above arguing for the second arm was
compatibility reasoning, and W-006 removed what it was protecting:

- *"The four `*_paed` types can never carry a pointer."* Two things killed this. It was **already
  false when written** — [T133] shipped twenty minutes after T134 and added the Form-tab control
  that sets exactly that pointer. And the types no longer exist: the dev database was rebuilt and
  they were operator rows in no seeder.
- *"It would have forced rewriting the test fixture ... the fixture is untouched."* **That reasoning
  was backwards.** `SeedActivityTypeAsync` created types with `SchemaJson` null — a shape nothing
  has been able to publish since T126. Every sampling test was passing through the family-name arm,
  so none of them proved the real CPSA tools are recognised as rated. An untouched fossil is not
  evidence; it is a restatement of the world the product had already left. The fixture now carries a
  real schema, with a separate unrated helper for the exclusion case.

A third reason surfaced that nobody had argued: the prefix arm made rated-ness a property of a
type's **name**, so a type keyed `cbd_checklist` would have entered a committee's evidence
denominator carrying no rating at all. A declared pointer cannot make that mistake — it names a
field, and the parser checks that field exists and is scale-typed.

The family map survives for **labelling only**, and lost two entries the College retired today:
`case_note_review` (D4: an alias of CCA) and `observed_clinical_exam` (D12: the same instrument as
Mini-CEX). It retires entirely with [T122]'s `WbaToolKey`.

The trajectory query now gates on the same declaration, which **closes its KNOWN LIMITATION**: an
institution's own rated tool charts because it says it is rated, not because a hard-coded list has
heard of its name. What is left there is cosmetic — its evidence source reads as its raw key.

New guard, and the one that was missing: `SeedScaleKeyTests.TheClassifierAgreesWithWhatTheSeedDeclares`
runs `Classify` over the **real seed corpus** rather than a fixture, plus
`ExactlyEightSeededToolsAreRated` naming all eight. Suites **994 → 1009**.

