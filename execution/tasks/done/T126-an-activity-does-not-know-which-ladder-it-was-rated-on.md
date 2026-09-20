---
id: T126
title: "Nothing can say which entrustment ladder a given activity was rated against"
status: done
priority: P2
created: 2026-09-19
started: 2026-09-20
completed: 2026-09-20
---
# T126 — Nothing can say which entrustment ladder a given activity was rated against

**Status:** done 2026-09-20
**Surfaced:** 2026-09-19, implementing [T123] defect 1. It is the reason that fix stops where it does.
**Severity:** Medium. Two planned behaviours both need this capability and neither can have it.
**Corrected 2026-09-20:** the original line read *"Nothing is wrong on screen today"*. That was false
and is now [T134] — `GetSamplingConcentrationWarnings` matches rated types by EXACT key over
`mini_cex`/`dops`/`cbd`/`acat`, so every v11.1 trainee's committee sampling report says there is no
rated evidence. Same seam, found while doing this task, filed separately.

## The gap

An `Activity` stores a rating as a bare ordinal inside `DataJson`. To say what that ordinal *means* you
need the ladder it was recorded against, which lives in the `scale_key` of the rated field in the
activity's **pinned** `ActivityTypeVersion.SchemaJson`. Two things stand between an arbitrary reader and
that value:

1. **Nothing knows which field carried the rating.** `GetEpaTrajectoryForTraineeQuery.TryParseObservation`
   reads the literal keys `overall` and `overall_level`; it never consults the schema. `CreditApplier` can
   find the field only because a credit directive names it in `MinimumLevelField`
   (`CreditApplier.cs:182-186`) — i.e. the answer is carried by the *credit rules*, not by the schema, and
   an activity type with no credit directive has no stated rated field at all.
   **Amended 2026-09-20:** `ListActivityTypesQuery.RatedScaleKeysOf` derives the same answer from
   `MinimumLevelField` independently — it shipped six minutes after this task was written, in the
   same session, which is why the task never said so. `CreditApplier` was not the only one.
2. **There is no navigation from an `Activity` to its pinned version.** `Activity` carries
   `ActivityTypeId` and `SchemaVersion` (`Activity.cs:9-10`); `ActivityTypeVersion` carries
   `ActivityTypeId` and `Version` (`ActivityTypeVersion.cs:6-8`). `ActivityService.GetPinnedVersion`
   (`ActivityService.cs:678-683`) matches them in memory off an already-loaded collection. There is no
   relationship to `Include`, so any query wanting it has to add a composite join by hand.

## What is blocked on it

**[T123] defect 1, resolution step 2.** The task specified a fallback: when the trainee's curriculum item
is unpinned, derive the axis from "the ladder the plotted activities themselves declare". That is
unimplementable without both of the above, so **it was not built**. The shipped behaviour resolves step 1
(the pinned `CurriculumItem.ScaleId`) and otherwise falls back to the numeric axis, which is what the
chart always drew. No trajectory got worse; one class of trajectory did not get better.

**[T123] DECISION 1b, in full.** D30 says a point whose own ladder differs from the axis should be drawn
hollow and left out of the polyline. What shipped marks a point whose **ordinal is not a rung on the axis
ladder** — which catches the visible subset and nothing else. The dangerous case is invisible to it: a
five-rung rating of 4 ("Independent" — the O-R Scale's order 4; the task said "Unsupervised", which is on no seeded rung) plotted against the six-rung CPSA axis is *within* 1–6, so it draws
as rung "3b" and looks entirely normal. Only the activity's own `scale_key` distinguishes them.

That case is structurally reachable and not merely operator error: `TryResolveSource`
(`GetEpaTrajectoryForTraineeQuery.cs:132-146`; the task cited :96-97, which moved) matches activity keys by **family prefix**, so
`mini_cex_paed` and `mini_cex_cpsa` chart on the same EPA axis by design. [T123] defect 3 removes the
legacy types from a v11.1 trainee's *menu*, which stops new ones being created; it does nothing to
activities already filed, and nothing to a trainee who moves curriculum.

## The shape of a fix

Two independent pieces, in this order:

1. **Declare the rated field in the schema**, not only in the credit rules. A `scale`-typed field is
   already distinguishable by `FormField.Type`; what is missing is a rule for which one is *the* rating
   when a type has several (the generic `mini_cex` seed has six `scale_key`-bearing fields). Options: a
   schema-level pointer like [T119]'s `observed_on` (the precedent, and it worked); the first `scale`
   field in declaration order (an inference, and this repository has been bitten by inference four times);
   or leave it to `MinimumLevelField` and accept that untyped-for-credit activities have no rated field.
   **The [T119] shape is the recommendation** — an explicit declared pointer, parsed *and* serialised,
   with a `SeedRoundTripTests` entry.
2. **A navigation from `Activity` to its pinned `ActivityTypeVersion`.** A composite FK
   (`ActivityTypeId`, `SchemaVersion`) → (`ActivityTypeId`, `Version`) would make it an `Include`. Nothing
   is live, so a destructive migration is available.

Then [T123] step 2 and full D30 are both small, and `TryParseObservation` stops hard-coding two field
names in a third place.

## Not urgent, and say why

Every trajectory renders correctly today for the case that matters: a curriculum-3 trainee gets the CPSA
axis with its own rung labels, because their items are 15/15 pinned. Curriculum 2 pins nothing and draws
the numeric axis it always drew. The gap only bites a trainee whose evidence spans two ladders, which
[T123] defect 3 now prevents at the point of creation. **This is worth doing before [T104] re-pins
anybody, and before any curriculum is pinned that has legacy activities already filed against it.**

## Related

Split out of [T123] defect 1 rather than left as an unmarked hole in it. Feeds [T110] (whose fix makes the
generic tools resolve to a ladder, at which point more activities have a knowable scale), [T122] (which
wants `WbaToolKey` on a type and would sit beside a declared rating field) and [T104]. The
`MinimumLevelField` asymmetry it describes is the same one [T124] flags under D6.

---

## Outcome — 2026-09-20

### What was built

1. **`FormSchema.RatedLevelField`**, the T119 shape exactly: a trailing optional record parameter,
   allow-listed at the parser root, parsed, **validated at parse time** (the named field must exist
   and must be `scale`-typed), and emitted by `Serialize` at a fixed position between
   `observation_date_field` and `sections`.
2. **Eight seeds declare it**, six abstain. The values match each seed's existing
   `minimum_level_field` exactly — `overall` on the four generic tools, `overall_level` on the four
   CPSA tools — which was re-derived from the seed files rather than assumed.
3. **A read-time resolver**, `ResolveRatedScaleIdsAsync`, giving each activity the scale its rating
   was recorded against, resolved from its **pinned** `ActivityTypeVersion`.
4. **D30's real rule** on `TrajectoryPointDto.OffLadder`, replacing the ordinal-range heuristic as
   the only signal. `TrajectoryChart` now ORs the two: the declared-ladder test catches the
   dangerous case, the ordinal test still catches an ordinal that is no rung on the axis at all.

### The composite FK in "The shape of a fix" was wrong, and was not built

Piece 2 above proposed a composite FK `(ActivityTypeId, SchemaVersion)` → `(ActivityTypeId, Version)`.
Three independent designs and three judges all rejected it, and the repository's own reason is
stronger than any of theirs. `ActivityConfiguration` records:

> *"these are read-path indexes, not FKs. No foreign key is declared deliberately — the columns are a
> snapshot of where the subject trained at creation, and a later restructure of the institution tree
> must not cascade into, or be blocked by, historical assessments."*

`SchemaVersion` is the same kind of snapshot column. There is also no `HasAlternateKey` anywhere in
the repository to point such an FK at, and `Program.cs` runs `MigrateAsync` unguarded, so a failed
constraint would stop the host booting.

**No migration was needed at all.** The decisive point is one both stamping proposals conceded and
then argued past: nothing filters, groups or sorts on a rating. T119's `ObservedOn` column exists
because it is a real SQL predicate with its own index; a rating is projected, never selected on. The
ladder is already on the pinned `ActivityTypeVersion` row, so it is read there.

### Deliberately NOT retired, and still true after this task

- **`TryResolveSource` gates first.** A declared pointer does not by itself make a type chart — the
  hard-coded family list still decides what is plotted at all, and that file's own KNOWN LIMITATION
  comment names the real fix as a flag on `ActivityType`, which is [T122]'s `WbaToolKey`.
- **The credit rules still answer the same question separately**, in `CreditApplier` and in
  `ListActivityTypesQuery.RatedScaleKeysOf`. They were not repointed at the new property: they ask
  which field a *particular directive* gates on, which may legitimately be one of the five or six
  component scales the generic tools declare. They coincide across the corpus today, and a new test
  pins that so the day they diverge is a decision rather than a discovery.
- **`GetSamplingConcentrationWarnings` has a third, exact-key list** that excludes the whole v11.1
  tool set. Filed as [T134].
- **T123 step 2** (deriving the axis from the activities' own ladders when the curriculum item is
  unpinned) was **not** built. It is now possible, but `ResolvePinnedLaddersAsync` returns early when
  the trainee has no profile or no curriculum item for the plotted EPAs — before any unpinned branch
  is reached — so the population step 2 is meant to serve is not the one the task describes. That
  needs its own thinking.

### Verification

- [x] `rated_level_field` parses, validates and serialises — `FormSchemaParserTests`, 9 new tests
      including both refusals and a second-round-trip fixed-point check
- [x] **T119's two refusals, never tested since it shipped, are now tested.** Before this task
      `FormSchemaParserTests` contained no occurrence of "observ" at all
- [x] Eight seeds declare the pointer, six abstain — `Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale`,
      a theory over every seed folder. **Verified to fail** by removing the pointer from `dops`:
      *"'dops' declares 6 scale field(s) (preparation, consent, technique, asepsis, post_procedure,
      overall) and nothing else can say which is the entrustment rating"*
- [x] The pointer agrees with each seed's credit directive — `Schema_RatedFieldAgreesWithTheCreditDirective`
- [x] Serialize half guarded by name — `RatedLevelField_SurvivesParseSerializeParse` and its mirror
- [x] A rating from another ladder is marked off-ladder **even when its ordinal is valid on the
      axis** — `ARatingFromAnotherLadderIsMarkedOffLadderEvenWhenItsOrdinalIsValidHere`. This is the
      case the task says is invisible today: a five-rung 4 draws as CPSA rung "3b" and looks normal
- [x] The ladder comes from the **pinned** version, not the newest — `TheLadderComesFromThePinnedVersionNotTheNewestOne`
- [x] An unresolvable or undeclared ladder is left alone rather than marked — a theory covering both
      the no-pointer and the `or_scale` cases. False means "no disagreement established"
- [x] **Verified to fail against the unfixed code**: disconnecting the resolver fails 2 of the 4
      positive tests; the other 2 assert the conservative default and correctly pass either way
- [x] `dotnet build Wombat.sln -c Release` — 0 warnings, 0 errors
- [x] Suites green, no `--no-build`: Domain **78**, Application **513**, Infrastructure **207**,
      Architecture **23**, Web **103** — **924 total**, up from 880

### Not verified

- **No browser check.** The chart change is a server-computed boolean feeding an existing CSS class
  (`is-off-scale`) at two call sites — `MyProgress.razor` and `ReviewDetail.razor`, the committee
  page. No new markup or styling. It has not been seen rendered.
- **No database was queried.** Whether any activity on dev or production is currently pinned to a
  version that would now resolve a ladder is unknown. Seeded types republish at next boot, which
  strands in-flight activities on their old version by design — they resolve nothing and are left
  alone rather than mis-marked.
