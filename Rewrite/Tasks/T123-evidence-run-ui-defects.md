# T123 — Three clinician-facing defects the first v11.1 run exposed: a five-point axis, two identical tools, and a menu from the wrong world

**Status:** defect 1 **DONE** 2026-09-19 (step 1 only — step 2 and the full D30 need [T126]).
Defect 3 **DONE** 2026-09-19. Defect 2 is a data edit awaiting D31, and defect 3's narrowing has
now closed it for trainees.

### Corrections applied 2026-09-19 while implementing

Several line references had drifted and two claims were wrong; recorded here so the argument above can
still be checked against the code.

- `CreditApplier.ResolveAchievedScaleIdsAsync` is at **`:177-225`**, `ResolveScaleIdAsync` at **`:232-247`**,
  the "identical on purpose" comment at **`:234-238`**, the `or_scale` remark at **`:169-176`**.
  `ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync` is **`:287-318`**, and the half that
  makes a numeric key work is `:296`, not the `:299-303` cited.
- **"Do not write a fourth copy" — there were two**, not three: `CreditApplier` and
  `ActivityReferenceDataService`. (`EntrustmentScaleReferences.cs:66` is a *name-only* matcher, a
  different and weaker rule — it cannot see a schema that binds by numeric id, i.e. every legacy `_paed`
  type, which is its own defect.) The id-or-name rule now lives once, in
  `Wombat.Application/Features/Epas/EntrustmentRungLabels.cs`, and the picker was re-pointed at it.
  `CreditApplier` keeps its own, named from the shared one.
- **`MinRating` was already a `[Parameter]`**, so `Rungs` overrides a parameter rather than a derived
  value. Precedence is now stated in the component: rungs win.
- **The grid loop was an integer stepper**, so `Rungs.Min/Max(Order)` would have assumed contiguity. It
  iterates the rung list instead, and a ladder with a deleted middle rung draws one tick per rung.
- 🚨 **`RenderYAxisLabel` emits a raw `MarkupString` with no HTML encoding** (`RenderXAxisLabel` next door
  encodes). Substituting an operator-editable `EntrustmentLevel.Label` into it, as D29 asks, was an
  injection hole the fix would have introduced. It now encodes, and there is a test.
- **`ChartPoint.Label` is not a rung label.** Both call sites pass `point.Source`
  ("Direct observation") and the column above it was headed "Source". The rung label needed a second
  field. Also: finding 8 is closed and the column is renamed **"Evidence type"**.
- **The profile rule has a trap.** `GetCurriculumProgressForTrainee.cs:56-58` — the *other* query on
  `MyProgress` — resolves the **active** profile, while `ResolveCreditableEpaIdsAsync` resolves
  active-first-then-latest and does **not** filter on `IsActive`. The chart follows the credit engine's
  rule, deliberately, and says so in a remark.

**Status (original):** open
**Surfaced:** 2026-09-19, the Wave-0 evidence run — `Rewrite/Tasks/T118-v11-1-evidence-run-findings.md`
findings 5, 6 and 7.
**Severity:** Medium-High. None of these corrupts data. All three are read or acted on by a registrar or an
assessor, and two of them route a trainee's evidence into the wrong curriculum world where it half-credits.

All three are separate fixes in separate files. They are in one task because they were found together and
because defect 3's fix is what actually disarms defect 2.

---

## Defect 1 — the rating trajectory draws a five-point axis for a six-rung ladder

### What is really happening

`src/Wombat.Web/Components/Shared/TrajectoryChart.razor:95-100`:

```csharp
private const int DefaultMaxRating = 5;

private int EffectiveMaxRating =>
  MaxRating ?? (Points.Count == 0
    ? DefaultMaxRating
    : Math.Max(DefaultMaxRating, Points.Max(point => point.Rating)));
```

T118 finding 5 says *"A rating of ordinal 6 (rung '5') has nowhere to plot."* That part is wrong and the
file says why: T085 already removed the hard ceiling, and the axis **grows** to fit an ordinal 6. The
finding's conclusion is right anyway, for a different reason: the axis floor is 5 and neither call site
passes `MaxRating` (`MyProgress.razor:60-61`, `ReviewDetail.razor:264-265`), so a CPSA trainee whose
ratings happen to sit at 3 sees `1 2 3 4 5` — a ladder with its top two rungs missing — until somebody
earns a 6 and the axis silently changes shape underneath them. The chart's own remark (`:87-92`) says the
default "grows to fit"; growing to fit the *data* is not the same as fitting the *scale*, and that
difference is the defect.

The labels are worse than the count. `RenderYAxisLabel:153-155` prints the bare ordinal, so a correct
six-point axis would read `1 2 3 4 5 6` against rungs `1, 2, 3a, 3b, 4, 5`
(`Seeds/paediatric-epa-v11.1.json`, scale `CPSA Paediatric Entrustment Scale v11.1`). Tick **5** would be
labelled 5 and mean rung **4**. This is the third surface of T100, and the one where the ordinal is most
actively misleading, because unlike the picker there is no adjacent list to disambiguate it. The
screen-reader table has the same problem at `:69` (`<td>@point.Rating</td>`), and an accessible equivalent
that is *less* informative than the picture is not an equivalent.

### The fix: the axis comes from the scale, resolved server-side

The component cannot resolve the scale — it has no trainee and no database. `GetEpaTrajectoryForTraineeQuery`
has both, and it already knows which trainee it is answering about. So `EpaTrajectoryDto`
(`GetEpaTrajectoryForTraineeQuery.cs:41-45`) gains the scale:

```csharp
public sealed record EpaTrajectoryDto(
    int EpaId, string EpaCode, string EpaTitle,
    int? ScaleId, string? ScaleName,
    IReadOnlyList<TrajectoryRungDto> Rungs,
    IReadOnlyList<TrajectoryPointDto> Points);

public sealed record TrajectoryRungDto(int Order, string Label);
```

Resolution, in order, per EPA:

1. **The trainee's `CurriculumItem.ScaleId` for that EPA** — T109's pin, and literally "the scale the EPA is
   pinned to". Resolve the profile exactly as `ActivityReferenceDataService.ResolveCreditableEpaIdsAsync`
   does (`:176-182`: active first, then latest `ProgrammeStartDate`, **not** filtered on `IsActive`, because
   a graduated trainee still credits), so the chart and the credit engine cannot disagree about which
   profile row is in force.
2. **Failing that, the ladder the plotted activities themselves declare** — the `scale_key` of the rated
   field in each activity's *pinned* `ActivityTypeVersion.SchemaJson`, resolved by id-or-exact-name. This
   is the resolution `CreditApplier.ResolveAchievedScaleIdsAsync:172-222` and `ResolveScaleIdAsync:230-243`
   already perform, and it is character-for-character the query in
   `ActivityReferenceDataService.GetEntrustmentScaleLevelOptionsAsync:299-303`. **Factor it — do not write
   a fourth copy.** T109's comment at `CreditApplier.cs:230-236` explains why the copies are identical on
   purpose; a fourth hand-written one is how they stop being. Use it only when every plotted point agrees
   on one scale.
3. **Otherwise `Rungs` is empty** and the component behaves exactly as it does today.

`TrajectoryChart` gains `[Parameter] IReadOnlyList<Rung> Rungs`. When it is non-empty, `MinRating` and
`EffectiveMaxRating` come from it (`Rungs.Min/Max(r => r.Order)`) and `RenderYAxisLabel` prints
`rung.Label`. When it is empty, every existing expression is untouched. Both call sites pass it through;
`MaxRating` stays as the manual override it already is.

### What happens to an existing five-rung trajectory

**Nothing, today.** Curriculum 2 is 0/15 pinned, so step 1 finds no scale. Its tools declare
`scale_key: "or_scale"`, which resolves to nothing at all — the only seeded scale is named `"O-R Scale"`
(T110; `CreditApplier.cs:164-171` records the same fact) — so step 2 finds no scale either. Every legacy
trajectory lands in step 3 and renders byte-identically to today. Curriculum 3 lands in step 1 and gets a
six-point axis labelled `1 2 3a 3b 4 5`.

That is the entire behavioural delta, and it is also the honest answer to the question: an existing
five-rung trajectory changes when, and only when, somebody pins its curriculum to a scale — at which point
the axis gains that scale's own rung labels, which is the point of pinning. If T110 is fixed first, the
four generic tools start resolving to the O-R Scale and every legacy chart gains a five-rung labelled axis
at that moment; that is correct, but it means **T110 and this defect change the same pictures**, and
whichever lands second should re-check the other's charts in a browser.

### 🔶 DECISION 1a (maintainer) — long rung labels on a 40px margin

CPSA's labels are one or two characters. The five-rung ladders' are words: "Independent", "Supervises
others". `LeftMargin = 40` (`:102`) on a 600×200 viewBox fits roughly five characters.

I would print the rung label on the axis when it is **four characters or fewer** and the ordinal otherwise,
and put the full label into the point `<title>` (`:45`) and into the screen-reader table's Rating cell
(`:69`) unconditionally — so the accessible copy is never poorer than the visual one. The alternative,
widening the margin to fit any label, costs plot width on a chart that already has to work at phone width
per `DESIGN.md`. Cheap to change later either way; it is a rendering rule, not a data model.

### 🔶 DECISION 1b (maintainer) — points on a different ladder from the axis

If a plotted activity's own `scale_key` resolves to a *different* scale from the axis, drawing them on one
y-axis is the T109 defect rendered as a picture: two incommensurable ordinals joined by a line that implies
a trend. Ndlovu can produce this today by filing a `mini_cex_paed` — see defect 3.

I would **keep such points, mark them hollow, and exclude them from the polyline**, with the table carrying
their own scale's rung label and a "different scale" note. Dropping them silently erases recorded evidence
from the page, which is the argument T108's `WithStoredValueAsync` (`ActivityReferenceDataService.cs:213-249`)
makes about the EPA picker and which applies identically here. The cheaper alternative — plot nothing when
the ladders disagree — is defensible but it hides a real observation behind a data-quality problem the
trainee did not create.

### While you are in this file: T118 finding 8 is not a defect

The trajectory table labels a `mini_cex_cpsa` as "Direct observation" because
`SourceByActivityFamily` (`GetEpaTrajectoryForTraineeQuery.cs:77-90`) maps `mini_cex`, `dops`,
`direct_observation` and `observed_clinical_exam` to one **evidence category**, deliberately. Close finding
8. The column header reads "Source", which is what invited the misreading — rename it **"Evidence type"**,
and consider carrying the tool name as well, which is what a clinician reading their own portfolio would
actually expect to see.

### Verification

- A curriculum-3 trainee with one rating of ordinal 3 sees a six-point axis labelled `1 2 3a 3b 4 5`.
- A curriculum-2 trainee's chart is pixel-identical to before the change.
- An unpinned curriculum whose tools all resolve to one scale gets that scale's axis (step 2).
- `TrajectoryChartTests.cs:54-70` (the growth case) still passes with `Rungs` unset.
- New: rungs supplied → axis count and labels come from them; rungs empty → today's axis; a cross-scale
  point is not in the polyline and is still in the table.

---

## Defect 2 — two indistinguishable "Mini-CEX (Paediatrics)", and two "DOPS (Paediatrics)"

Ids 11 `mini_cex_paed` / 17 `mini_cex_cpsa`, and 12 `dops_paed` / 18 `dops_cpsa`. The CPSA names are in
code (`ActivityTypeSeedCatalogue.cs:100-111`); the `_paed` names are operator data, built during the
scenario replay — T104 verified that a repo-wide grep finds `_paed` only in comments, tests and the
scenario documents. So one of each colliding pair lives in the repository and the other does not.

### Argument: this is not a labelling problem, and T104 is the real answer — but neither is what to do first

**A label cannot carry the distinction.** What a trainee would need to know is not which speciality owns
the tool (both say Paediatrics) but *which curriculum world will credit it*, and that is a property of the
**trainee**, not of the type. The correct label therefore differs per viewer, which a static name cannot
express. Appending the scope — "Mini-CEX (Paediatrics, KGK)" versus "(Paediatrics, CPSA)" — names the
owner, not the consequence, and `/activities/new` never tells a registrar which curriculum version they are
on, so the label would be a riddle with the answer withheld.

**The wrong choice is not harmlessly uncredited — it is half-credited, which is worse.**
`CreditApplier.ResolveCurriculumItemsAsync` (`:246-285`) scopes items by `trainee.CurriculumId`, so a
`mini_cex_paed` filed by Ndlovu against a curriculum-3 EPA **does** resolve item 17 and **does** increment
`CountsSoFar`. Its rating sits on the five-rung ladder against a six-rung pin, so T109 refuses the minimum
and stamps `ScaleMismatchCount`. Volume counts, progression does not, and the only signal is a banner the
trainee has to open the activity to see. No name prevents that; only not offering the tool does.

**So defect 3's narrowing is the fix.** Once it lands, a curriculum-3 trainee sees one Mini-CEX and a
curriculum-2 trainee sees the other. The collision survives only where the subject cannot be resolved at
all, where the fallback is deliberately permissive.

**T104 remains the real answer for the estate**, and this is a reason to bring it forward: two live worlds
under one discipline means `/admin/activity-types` shows the duplicate pair for ever. That surface has no
subject and cannot be narrowed — `ListActivityTypesAdminQuery` (`:23-53`) deliberately shows an admin
everything. But T104 is a hand-run data migration with no code to replay it on production, and it is
**blocked by the T109 re-pin** for the five trainees still on curriculum 2. It cannot be the fix that ships
this week.

### 🔶 DECISION 2 (maintainer) — the interim rename, and which side gets renamed

The one-line mitigation for the admin surfaces is to rename, through `/admin/activity-types`, on dev and on
production:

- **Rename the legacy four** (`Mini-CEX (Paediatrics — FCPaed legacy)` etc.). They are operator rows, so
  this is a data edit with no code change, and it marks the world that is scheduled for retirement.
- **Rename the CPSA four**, which would need a code change *and* a hand edit, because
  `ActivityTypeSeedRefresher` "never touches scope, name or description"
  (`ActivityTypeSeedCatalogue.cs:63-66`) — editing the catalogue would fix a fresh install and do nothing
  to dev or production.
- **Neither**, and rely on defect 3.

**I would rename the legacy four and change no code.** It costs four text edits, it makes every admin
surface unambiguous immediately, and it names the world that is going away rather than decorating the one
that is staying. Record it in the T104 production runbook, because this data does not exist in code and
nothing replays it.

---

## Defect 3 — a v11.1 trainee is offered the four legacy institution-scoped types

`ListActivityTypesQuery.cs:22-38` filters on the **type's** scope against the **caller's** claims, and on
nothing else:

```csharp
activityType.Scope == ActivityScope.Global ||
(activityType.Scope == ActivityScope.Institution && institutionId.HasValue && activityType.ScopeId == institutionId.Value) ||
(activityType.Scope == ActivityScope.Speciality && specialityIds.Contains(activityType.ScopeId ?? 0)) ||
(activityType.Scope == ActivityScope.SubSpeciality && subSpecialityIds.Contains(activityType.ScopeId ?? 0))
```

Ndlovu's claims still carry institution 2, so `mini_cex_paed`, `dops_paed`, `procedure_log_paed` and
`msf_paed` (`Scope = Institution, ScopeId = 2`) are offered although their world is not hers. The
trainee's curriculum is not a term anywhere in this query.

### How T108 narrowed the EPA picker, and what carries over

T108's four principles are all reusable and all stated in
`ActivityReferenceDataService.cs:49-79` and `:148-166`:

1. Narrow by what would actually **credit**, mirroring the engine rather than inventing a second rule.
2. Resolve against the **subject**, not the viewer.
3. **Replace** the claims filter rather than intersecting it, because the two answer different questions
   and intersecting can only over-hide.
4. **Fall back permissively** wherever the answer cannot be resolved, because *"an empty picker on a
   required field is unsubmittable, which is a worse failure than the one this task is about."*

Principle 1 does **not** separate the two paediatric worlds, and this is the trap. Both `mini_cex_paed` and
`mini_cex_cpsa` declare `epa_field: "epa_id"` and curriculum 3 has items, so "could this type credit
anything for this subject?" is **true for both**. Narrowing on creditability alone changes nothing.

### The predicate that does separate them: T109 option 2, unshipped

What distinguishes the two worlds is the **ladder**: the type's rated field's `scale_key` versus the scale
the subject's curriculum items are pinned to. That is precisely the option T109 recommended and did not
ship — it shipped option 1:

> **2. Refuse the combination at source.** Do not offer an activity type whose `scale_key` resolves to a
> different scale from the subject's curriculum's. […] it is the same predicate T108 applied to EPAs,
> applied one level up to `ListActivityTypesQuery`.
> — `T109-cross-scale-credit-comparison.md`, Options; *"Recommendation: **2 now** […] then **1**"*

So:

> Offer activity type **T** to subject **S** iff `T.Scope` matches S's claims (today's filter) **and**
> either T declares no resolvable rating ladder, **or** S's curriculum pins no ladder, **or** the two are
> the same scale.

Behaviour on today's data, type by type:

| Type | Declared ladder | Curriculum 3 (pinned, 6-rung) | Curriculum 2 (0/15 pinned) |
|---|---|---|---|
| `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa` | `"CPSA Paediatric Entrustment Scale v11.1"` → resolves | offered (same scale) | offered (nothing pinned) |
| `mini_cex`, `cbd`, `dops`, `acat` (generic) | `or_scale` → resolves to nothing (T110) | offered (unresolvable) | offered |
| `reflective_note`, `procedure_log`, `journal_club`, … | no rated field | offered | offered |
| `mini_cex_paed`, `dops_paed` | **unverified — operator data** | **the whole question** | offered |

**Nothing that is offered today stops being offered by accident**, and no legacy trainee's menu changes at
all, because curriculum 2 pins nothing.

### ⚠️ Verify before calling this done

`mini_cex_paed` and `dops_paed` were built through the visual builder during the scenario replay, so their
`scale_key` is whatever the operator typed. Run this against dev before relying on the predicate:

```sql
select id, "Key", "Name", "Scope", "ScopeId", "SchemaJson"
  from "ActivityTypes"
 where "Key" in ('mini_cex_paed','dops_paed','procedure_log_paed','msf_paed');
```

- If `scale_key` names the five-rung "Paed General Entrustment Scale", the predicate drops them from
  Ndlovu's menu and defect 3 is closed by code.
- If it is blank, or a key like `or_scale` that resolves to nothing, the predicate **falls through
  permissively by design** and they stay on the menu. In that case the only remedies are T104 and defect
  2's rename, and this task should say so rather than claim a fix it did not make.

The same query answers whether `procedure_log_paed` and `msf_paed` are rated at all; neither is expected to
be, and neither would ever be narrowed.

### Wiring

`ListActivityTypesQuery` gains `string? SubjectUserId`. It has exactly **one** caller —
`NewActivity.razor:78` — where the creator *is* the subject, which that page's own comment already states
for the EPA picker (`:42-43`: *"The creator IS the subject here […] so the EPA picker narrows to what would
actually credit for them (T108)"*). Pass `_currentUserId`, which the page already holds for that reason
(`:63-65`).

Resolve against `ActivityType.SchemaJson` — the current **published** schema, not a pinned version — because
the trainee is about to create a *new* activity at the current version. (Contrast `CreditApplier`, which
must read the pinned version because it is scoring an activity that already exists.) Both are right; the
difference is worth a comment at the call site so the next reader does not "fix" one into the other.

### What this does not fix, deliberately

- `ListActivityTypesAdminQuery` is untouched. An administrator must see every type, including the ones no
  trainee is offered.
- A type with no rated field is never narrowed, so a v11.1 trainee still sees `procedure_log_paed` and
  `msf_paed` if those declare no scale. Defect 2's rename covers the confusion; T104 removes them.
- This narrows by ladder, not by tool. Narrowing by *which EPAs a tool may credit* is [T122], and the two
  compose: once both land, a v11.1 trainee sees four CPSA tools, and within each, only the EPAs that tool
  is permitted to credit. **Both must keep the permissive fallback, or between them they will empty a
  picker** — the failure T108 explicitly guarded against and still nearly shipped (see its adversarial
  review, finding 1).

### Verification

- Ndlovu (curriculum 3) is offered the four `_cpsa` types, the generic types and no `_paed` type —
  subject to the SQL check above.
- A curriculum-2 trainee's menu is unchanged, item for item.
- A `PendingTrainee` with no `TraineeProfile` gets the full claims-scoped list, not an empty one.
- A trainee whose curriculum has no items gets the full list.
- The builder preview and `/admin/activity-types` are unchanged.

---

## Related

Findings 5, 6 and 7 of [T118]. Defect 1 is a third surface for [T100] (ordinal-versus-rung) and moves with
[T110] (the `or_scale` mismatch), which changes the same charts. Defect 3 ships [T109]'s unshipped option 2
and is the natural companion to [T108]'s EPA narrowing; it composes with [T122]. Defect 2 argues for
bringing [T104] forward and records the interim rename it needs; T104 itself stays blocked on the T109
re-pin. T118 finding 8 is closed here as not-a-defect.
