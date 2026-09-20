---
id: T134
title: "Committee sampling reports zero rated evidence for every v11.1 trainee"
status: queued
priority: P1
owner: agent
depends_on: []
created: 2026-09-20
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
