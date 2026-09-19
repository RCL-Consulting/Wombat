# T126 — Nothing can say which entrustment ladder a given activity was rated against

**Status:** open
**Surfaced:** 2026-09-19, implementing [T123] defect 1. It is the reason that fix stops where it does.
**Severity:** Medium. Nothing is wrong on screen today; this is a missing capability that two planned
behaviours both need and neither can have.

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
five-rung rating of 4 ("Unsupervised") plotted against the six-rung CPSA axis is *within* 1–6, so it draws
as rung "3b" and looks entirely normal. Only the activity's own `scale_key` distinguishes them.

That case is structurally reachable and not merely operator error: `TryResolveSource`
(`GetEpaTrajectoryForTraineeQuery.cs:96-97`) matches activity keys by **family prefix**, so
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
