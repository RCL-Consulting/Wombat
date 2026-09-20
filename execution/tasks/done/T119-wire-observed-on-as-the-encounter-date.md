---
id: T119
title: "The encounter date the clinician types is discarded, and everything is dated by the audit clock"
status: done
priority: P1
created: 2026-09-19
---
# T119 — The encounter date the clinician types is discarded, and everything is dated by the audit clock

**Status:** open
**Surfaced:** 2026-09-19, [T118] finding 4 — the first real v11.1 assessment recorded an encounter on
**2026-03-10** and the product plotted it on **2026-09-19**.
**Severity:** High, and **blocking**. The display half is clinician-facing and wrong today. The credit half
is latent and silently wrong the moment an encounter and its paperwork fall in different training years.
Nothing in [T098] phase 3 can start until this lands, because phase 3 buckets completions by date and there
is no trustworthy date to bucket on.

## Symptom

`mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa` and `direct_observation_cpsa` all declare an `observed_on` field
whose own help text reads *"When the encounter happened, not when this form is completed."*
(`src/Wombat.Infrastructure/Activities/Seeds/mini_cex_cpsa/schema.json:21-27`). The value is validated, it
is stored in `DataJson`, and **no C# anywhere reads it**. Every date the product shows or reasons with
comes from `Activity.CreatedOn` — the row's audit timestamp.

The product asks for the date, promises it matters, and throws it away.

A second, smaller defect rides along: `CreatedOn` is `DateTime.UtcNow`
(`src/Wombat.Infrastructure/Activities/ActivityService.cs:79`) and every reader renders it with
`DateOnly.FromDateTime(...)`, i.e. the **UTC** calendar day. South Africa is UTC+2 year round, so an
assessment submitted at 00:30 SAST is stamped 22:30 the previous day and displays a day early. That
disappears for any activity with a declared encounter date and survives for the ones that fall back.

## Every place the encounter date is used today

Established by reading each site, not by grepping alone. The middle column is what the site is really
asking.

| Site | The question it is asking | Today |
|---|---|---|
| `CreditApplier.cs:56-57`, `:335-349`, `:377` | *What programme stage was the trainee in when this happened?* — selects the curriculum item's effective minimum via `GetMinimumLevelForStage` (`CreditApplier.cs:303`) | `ResolveObservationDate` → `CreatedOn`, then earliest transition, then `UtcNow` |
| `GetEpaTrajectoryForTraineeQuery.cs:158` | *Where on the x-axis does this observation sit?* | `DateOnly.FromDateTime(activity.CreatedOn)` |
| `GetEpaTrajectoryForTraineeQuery.cs:134,140` | *Which observations fall in the requested window?* | SQL filter on `CreatedOn` |
| `PortfolioPdfService.cs:151,157,162` | *Which activities belong in this portfolio period?* | SQL filter and sort on `CreatedOn` |
| `ActivitiesSectionComponent.cs:71` | *What date does the exported PDF print beside this WBA?* | `CreatedOn.ToString("yyyy-MM-dd")` |
| `StartCommitteeReview.cs:63-64` | *What evidence falls inside the review period?* | SQL filter on `CreatedOn` |
| `GetSamplingConcentrationWarnings.cs:134-135` | *Is this trainee's evidence bunched at one assessor / one month?* | SQL filter on `CreatedOn` |

Three further sites compute a stage but are asking about **now**, not about an encounter, and are
deliberately untouched by this task:

- `GetTraineeDashboardSummaryQuery.cs:69,150` and `GetCurriculumProgressForTrainee.cs:66-67,120` — both
  call `ComputeTraineeStage(profile.ProgrammeStartDate, today)` to answer *"what is expected of me this
  year?"*. Today is the right input there.
- Note the consequence, because it is going to look like a bug: after this task, the minimum a progress
  page **displays** (today's stage) and the minimum a completion was **graded against** (the encounter's
  stage) can legitimately differ. [T118] finding 2 already shows that line is confusing before it even has
  two possible values. Whoever fixes T100 should make the progress page say which stage a number belongs
  to.

And four sites print or order by `CreatedOn` as a record-keeping fact — `ListActivitiesBySubjectQuery.cs:51`,
`ListActivitiesByActorInboxQuery.cs:56`, `GetTraineeDashboardSummaryQuery.cs:98-101`,
`AccessReportBuilder.cs:73`. Those stay: "when was this record created" is a real and separate question,
and the data-rights report in particular should keep showing the audit timestamp.

MSF carries its own campaign dates and no `EpaId`; it is out of scope here and belongs with the MSF credit
task.

## Design — where does the encounter date come from?

### Option (a): a well-known field key, by convention

Read `DataJson["observed_on"]` and be done. Zero DSL work.

**Rejected, and the seed corpus is the evidence.** The fourteen seeded types name their date field four
different ways, and four of them have no date field at all:

| Key | Seeds |
|---|---|
| `observed_on` | `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa` |
| `performed_on` | `procedure_log` |
| `session_date` | `journal_club` |
| `activity_date` | `research_output` |
| *(none)* | `mini_cex`, `dops`, `cbd`, `acat`, `teaching_session`, `reflective_note`, `qi_project` |

A convention would silently ignore three types that do carry a real date, and would silently do nothing
for an institution that builds its own tool and calls the field `encounter_date`. Silent, unsignalled
degradation of exactly this shape is the failure mode this repository has now hit four separate times
(`or_scale` resolving to nothing in T109, the seeder skipping existing keys in T103, a DSL property parsed
but never serialised, `observed_on` itself). We should stop adding more of it.

### Option (b): a DSL directive naming the field

Make the type say which of its fields is the encounter date. Correct, explicit, and checkable at publish
time.

**Necessary but not sufficient on its own.** If the answer lives only in the pinned schema, then answering
"what date is this activity?" means parsing that activity's pinned `SchemaJson` and reaching into its
`jsonb` — per activity, per read. Four of the seven sites in the table above filter or sort **in SQL over
many rows** (`PortfolioPdfService`, `StartCommitteeReview`, `GetSamplingConcentrationWarnings`, the
trajectory's `From`/`To`), and phase 3 will `GROUP BY` period over the whole corpus. A per-type field name
inside `jsonb` is not something those queries can express, let alone index.

### Option (c): a real `Activity.ObservedOn` column

**This is the T101 precedent, and it was chosen for these exact reasons.** `Activity` gained
`InstitutionId` / `SpecialityId` / `SubSpecialityId` as stamped columns rather than deriving scope on every
read; the reasoning is written into `src/Wombat.Domain/Activities/Activity.cs:19-52` — *"stamped rather
than derived so that authorization is a column comparison rather than a three-table join per read"*. The
same argument applies here with more force, because dates get ranged and grouped, not just compared.

But a column alone cannot know **which** field to stamp from. That is option (b)'s job.

### Recommendation: (b) declares it, (c) stores it

They are not competing options; they are the two halves of one answer, and either alone is broken.

> **The pinned schema names the field. `ActivityService` resolves it and stamps a column at every write.
> Every reader reads the column.**

One resolver, one stored answer, one thing to query. This is the `ActorRuleMatcher` lesson from T070
restated — *"the one implementation of the actor grammar … shared by transition authorization and field-write
authorization so the two cannot drift"* (CLAUDE.md). There must be exactly one implementation of "what
date did this happen".

## The DSL addition

A **root-level pointer on the form schema**, not a per-field flag and not a credit-rules property:

```json
{
  "version": 1,
  "observation_date_field": "observed_on",
  "sections": [ … ]
}
```

- **Root, not per-field** (`"is_observation_date": true`) — a root pointer is single by construction. A
  per-field flag lets two fields claim the role and forces the parser to police it.
- **Schema, not credit rules.** `procedure_log`, `journal_club` and `research_output` declare an empty
  `counts_for` and credit nothing, but their dates still drive the portfolio export, the committee review
  window and the sampling warnings. An encounter date is a property of the form, not of crediting.

Work required, in order, because the second step is the one that gets forgotten:

1. `FormSchema` (`src/Wombat.Domain/Activities/Schema/FormSchema.cs:3-5`) gains `string?
   ObservationDateField`.
2. `FormSchemaParser.Parse` — add `observation_date_field` to the allowed root properties at
   `FormSchemaParser.cs:148`; the allow-list throws on anything unknown, so without this every existing
   seed carrying the pointer fails to parse.
3. **`FormSchemaParser.Serialize` must emit it** (`FormSchemaParser.cs:23-31`). `ActivityType.SaveDraft`
   round-trips Parse+Serialize (`ActivityType.cs:51`), so a property with no `Serialize` half is dropped
   at publish with no error: the JSON parses, the builder shows the setting, the feature dies silently.
   CLAUDE.md names this trap explicitly. Add the case to
   `tests/Wombat.Infrastructure.Tests/Activities/SeedRoundTripTests.cs`.
4. **Publish-time validation**, in the parser: the named key must exist somewhere in `sections`, and its
   `type` must be `date`. A pointer at a missing field, or at a `datetime`/`text` field, is refused with a
   message naming the key. `datetime` is refused rather than truncated — every candidate field in the
   corpus is a `date`, and accepting both invites a timezone argument we do not need.
5. `BuilderFieldModel` / `BuilderSectionModel` need no change (this is a root property), but
   `ActivityTypeEdit.razor`'s form tab must carry it through the builder round-trip or an admin who saves
   from the builder silently drops it — the same hazard T070 hit and fixed
   (`BuilderModels.cs:241-244`). A visible input on the Form tab is the right finish; carrying it
   through unharmed is the minimum.

### Seed edits that go with it

| Seed | Pointer |
|---|---|
| `mini_cex_cpsa`, `dops_cpsa`, `cbd_cpsa`, `direct_observation_cpsa` | `observed_on` (field already exists) |
| `procedure_log` | `performed_on` |
| `journal_club` | `session_date` |
| `research_output` | `activity_date` |
| `mini_cex`, `dops`, `cbd`, `acat`, `teaching_session` | **add** an `observed_on` date field, `required: true`, then point at it. An assessment with no encounter date is the defect this task exists to remove |
| `reflective_note`, `qi_project` | **no pointer.** A reflection and a months-long QI project have no single encounter date; they fall back, and neither credits anything |

`ActivityTypeSeedRefresher` (T103) carries all of these into the existing dev and production databases on
the next boot, and in-flight activities stay pinned to their old version — which is correct and is why the
restamp below exists.

## The column, and when it is stamped

```csharp
public DateOnly ObservedOn { get; set; }                 // never null
public ObservationDateSource ObservedOnSource { get; set; }
```

```csharp
public enum ObservationDateSource { Declared = 0, CreatedOn = 1 }
```

**Non-null, with provenance beside it.** The obvious alternative — nullable, with every reader
coalescing — was rejected for one concrete reason: a nullable column pushes
`COALESCE(observed_on, created_on::date)` into four SQL query sites and into every phase-3 `GROUP BY`,
which is both an index problem and four chances to forget. A non-null column is directly indexable and
directly groupable.

The three-valued information that nullability would have carried is not lost, it is moved into
`ObservedOnSource`, which is the same device T109 used for `UnverifiedLevelCount` — *"the exposure meter:
a non-zero value says this tally rests on ordinals nobody has verified"*
(`src/Wombat.Domain/Curricula/CurriculumItemProgress.cs:28-33`). Here it says: *this date is the audit
clock, not a clinician's statement*. It is how a surface can mark a trajectory point as undated evidence
rather than pretending September is a fact, and how an operator can count how much of the corpus is still
guessing.

### Resolution

```
pinned schema declares observation_date_field
  AND DataJson holds a parseable DateOnly at that key   → (that date,           Declared)
otherwise                                                → (CreatedOn.ToDateOnly, CreatedOn)
```

The **pinned** version, resolved through `GetPinnedVersion(activity)` — not the live published schema —
for the reason T109 already wrote down: pinning means the binding cannot drift under an activity that is
already in flight.

### Stamped at every write, not once at creation

Three sites, all in `ActivityService`, all the same one-line call:

| Site | Why it must restamp |
|---|---|
| `CreateDraftAsync` (`ActivityService.cs:43-125`, beside the T101 scope stamp at `:76-79`) | the initial value |
| `UpdateDraftAsync` (`:126-152`, after `ThrowIfInvalid` at `:146`) | a trainee correcting the date before submitting would otherwise leave a stale stamp |
| `TransitionAsync` (`:153-249`) — **after** the writable-key merge at `:181-195`, **before** `ApplyTransition` at `:209` | the `submit` transition can carry a `DataPatchJson` that changes the date, and `CreditApplier` runs later in the same method at `:232` and reads the stamp |

The ordering inside `TransitionAsync` is load-bearing twice over: stamping before `ApplyTransition` keeps
the column and the transition's `SnapshotJson` in agreement, and stamping before the credit call at `:232`
is the entire point of the task.

For the CPSA seeds the restamp on `complete` is a no-op — `observed_on` declares no `editable_by`, so it
falls back to the `subject|creator` default (`FieldPermissionEvaluator.cs:18,46`), which conjoined with the
`requested` state's `editable_by: field:assessor_user_id` means nobody can write it after submission. It
still runs, so that a type which *does* let the assessor set the date works without a special case.

### Persistence

`ActivityConfiguration.cs` gains `builder.HasIndex(entity => entity.ObservedOn);` beside the existing
`CreatedOn` index at `:21`. `DateOnly` maps to `date` by default under Npgsql, as
`TraineeProfile.ProgrammeStartDate` already does with no explicit configuration.

Hand-written migration `20260919xxxxxx_T119_ActivityObservedOn`, **with its `.Designer.cs`** and a
`ApplicationDbContextModelSnapshot.cs` update — CLAUDE.md: without the Designer file EF will not discover
the migration and `MigrateAsync()` silently skips it.

## The fallback, and the eleven existing rows

The migration adds the columns and backfills **every** row with
`"ObservedOn" = ("CreatedOn" AT TIME ZONE 'UTC')::date` and `"ObservedOnSource" = 1` (`CreatedOn`).

It deliberately does **not** try to read `DataJson`, and the reason is a startup-ordering fact worth
checking before anyone argues otherwise: `Program.cs:473` runs `MigrateAsync()`, the seeders run at
`:480-483`, and `ActivityTypeSeedRefresher` runs at `:488`. At migration time **no published version
declares `observation_date_field` yet** — the refresher has not run. The migration cannot resolve the
pointer, so any date it pulled out of `jsonb` would have to come from a hard-coded key list: option (a),
smuggled in through the back door.

So the resolution happens where the pointer exists, in C#:

**`ActivityObservationDateRestamper`, invoked from `Program.cs` immediately after the refresher at `:488`.**
It selects activities with `ObservedOnSource = CreatedOn`, re-runs the *same* resolver against each one's
pinned schema, and writes back the ones that now resolve. Idempotent, cheap (one filtered query), and it
reuses the runtime resolver rather than re-implementing it. Gate it on
`WombatOptions.RestampActivityObservationDates` (default `true`), mirroring
`RefreshSeededActivityTypes` at `src/Wombat.Application/Common/Options/WombatOptions.cs:26`, and log a
count.

Note `src/Wombat.Api/Program.cs:26` migrates but runs neither the seeders nor the refresher, so the
restamper belongs in the Web host only — the same placement the refresher already has.

On dev this converts exactly one row: activity 12, whose `DataJson` holds `observed_on` `2026-03-10`
([T118] "Evidence left in the dev database"). The other eleven have no declared date on any pinned version
and stay on the fallback for ever, correctly labelled as such. That is the right outcome: **do not
manufacture clinical history.** Nothing is live, so if the eleven become annoying they can be regenerated
rather than invented.

## The consequence that needs care: re-dating T073's per-stage minimum

This is the part that can quietly change what a trainee's record says.

### What happens to rows that are already credited: nothing, automatically

`CurriculumItemProgress` stores **tallies**, not per-activity outcomes, plus a `CreditedActivityKeysJson`
dedupe set (`CreditApplier.cs:115,153`). Changing an activity's date does not touch a stored row, because
nothing re-reads it. Worse — it cannot be topped up: `GetCreditKey` (`CreditApplier.cs:395-403`) returns
`{activityId}:{transitionKey}`, so re-running credit over an already-credited activity is a no-op by
design. **The only thing that can change a stored tally is a full rebuild.**

So after this task ships, without further action:

- every completion from tomorrow is graded against the stage its **encounter** falls in — correct;
- every completion already recorded stays graded against the stage its **paperwork** fell in — frozen, and
  in general wrong.

### How wrong, on the data that exists

One credited row exists on dev: Ndlovu, curriculum item 17, from activity 12. Both candidate dates
(2026-03-10 and 2026-09-19) fall in stage 1, so the stage minimum was 3 either way and **the stored tally
would not change**. [T118] established this. The dev corpus therefore needs no repair, and the risk is
theoretical *today* and permanent from tomorrow.

In general the direction is not guaranteed. Moving a date earlier usually lands on an earlier stage with a
lower minimum, so re-dating tends to *grant* credit that was refused. But nothing enforces monotonicity on
`MinimumLevelByStageJson` — `CurriculumItem.ParseStageOverrides` just reads a dictionary — so a
non-monotone override set can move it the other way and withdraw a `MinimumLevelReachedCount` a trainee
already sees.

### Should anything be recomputed? Yes — once, deliberately, and not by the restamper

The restamper must **not** re-credit. Mixing "fix the date" with "re-score history" in one unattended
startup step means a boot silently changes a trainee's recorded progress with no operator action and no
record of what moved.

The recompute belongs to `RebuildCurriculumProgressCommand`, run **once** at the end of this task's
session. Read the handler before trusting it — it has three problems:

1. **It has no caller.** Grep confirms it: the only references are its own file and
   `tests/Wombat.Architecture.Tests/ActivityReadBoundaryTests.cs:60`. It is unreachable from the product.
   It needs a trigger — an Administrator-only button on an admin page, behind a confirmation that says
   what it will do.
2. **It is not atomic.** `RebuildCurriculumProgressCommand.cs:30-32` deletes every `CurriculumItemProgress`
   row and **saves**; the replay saves separately at `:71`. A failure, a cancellation or a deadlock in
   between leaves **every trainee in the system with zero progress** and no way back except running it
   again. Wrap the whole handler in one transaction before giving it a button. This is a defect in its own
   right and it is cheap to fix here; if it is not fixed, do not add the button.
3. **It does not stamp `CreditedItemCount`** ([T106] item 12), so a rebuild leaves T108's stale zeros on
   the transitions it just re-credited, and the "this credited nothing" banner stays on for ever after the
   exact remediation it recommends. Riding that fix along is a few lines against the same `ICreditApplier`
   return; otherwise the browser check below will show a banner that contradicts the data.

Order for the session: **ship the stamp → boot (migration + refresher + restamp) → fix and run the rebuild
→ verify.**

## Bounding backdating

There is no bound today: `SchemaValidator.ValidateDateField` (`SchemaValidator.cs:166-172`) checks only
that the string parses, `FieldValidation` carries `min`/`max` for numbers only, and the form renders a bare
`<input type="date">` (`ActivityForm.razor:50-52`). A trainee can claim an encounter in 2019, or in 2031.

Two hard rules, enforced at the stamp site in `ActivityService` where the subject's `TraineeProfile` is
already being loaded for the T101 scope stamp, and reported as an ordinary field error through
`ThrowIfInvalid` (`ActivityService.cs:672`) so the form shows it against `observed_on` rather than throwing
a page-level exception:

- **Not in the future.** An encounter that has not happened cannot be assessed.
- **Not before `TraineeProfile.ProgrammeStartDate`.** An encounter before the programme began cannot count
  toward it — and `GetStage` returns `null` for such a date (`TraineeProfile.cs:66-71`), which silently
  drops the activity to the flat `MinimumLevelOrder` instead of a stage minimum. That silent downgrade is
  reason enough on its own.

Both are derivable from data the product already holds and need no policy input. A subject with no trainee
profile gets the future check only.

Deliberately **not** a hard rule: an age limit. Refusing a four-month-old encounter does not produce timely
paperwork, it produces a registrar who types today's date to get past the validator — which is precisely
the defect this task is repairing, re-created by its own guard. Ship a **soft warning** beyond
`WombatOptions.BackdatingWarningDays` (default 90) that is shown on the form and recorded on the
transition, and leave the hard limit to the College (**D3** below).

`CurriculumItem.WindowMonths` is **not** the answer here and must not be pressed into service: it is
per-item currency, it is read by nothing in the credit path, and its only consumer is
`AdmitTrainee.cs:126` ([T106] item 10). Different question, different column.

## Decisions required

Marked so they can be collected. None of them blocks starting the implementation; all of them change a
default that is already chosen below.

**D1 — Do the seeded generic WBA tools get a required encounter date?**
`mini_cex`, `dops`, `cbd`, `acat` and `teaching_session` have no date field at all. Adding one as
`required: true` means any new activity of those types must carry it; existing in-flight ones stay pinned
and are unaffected. The alternative is to leave them on the `CreatedOn` fallback, which is honest but keeps
five assessment tools permanently undated.
*Recommendation: add it.* They are assessments; an assessment without an encounter date is not evidence of
anything. The types belong to the demo General Medicine speciality and are due for review under [T104]
anyway, so the blast radius is dev scenario data.

**D2 — Does the encounter date appear in the portfolio PDF, the committee review window, and the sampling
warnings, or only in credit and the trajectory?**
The table above assumes **yes, all of them** — a review period and a portfolio period are clinical
periods, not filing periods. The cost is that a committee reviewing "2026 H1" will now see an encounter
filed in September if it happened in March, which is correct but is a visible change to what a panel is
shown. The cheaper scope is credit + trajectory only.
*Recommendation: all of them, in this task.* Leaving `PortfolioPdfService` and `StartCommitteeReview` on
`CreatedOn` means two dates in one product with no rule for which is which — exactly the ambiguity T119
exists to remove.

**D3 — Is there a College deadline for submitting an assessment after the encounter?**
Annexure A and B are silent as read. If the CMSA requires, say, "logged within 30 days", it should be a
hard refusal rather than a warning, and it probably belongs per College rather than as a constant.
*Recommendation: ship the soft 90-day warning, ask the College, and make it a hard rule only if they say
so.* A hard rule invented here will be worked around by typing a false date.

**D4 — Should a `CreatedOn`-sourced date be visibly marked to the reader?**
`ObservedOnSource` makes it possible: a trajectory point or a PDF line could read *"date not recorded —
filed 2026-09-19"* instead of silently asserting a date. Costs a small UI pass across the trajectory table,
the PDF activities section and the activity view.
*Recommendation: yes, but as a follow-up.* It is a display concern and it will read better once T100 has
fixed the neighbouring label defects.

## What this task explicitly does **not** decide

`TraineeProfile.GetStage` stays exactly as it is — 365-day blocks from `ProgrammeStartDate`. The
academic-year decision recorded in [T098] means stage and period now disagree by construction, and
reconciling them is that task's obligation 1, not this one's. **T119 decides what date an activity has;
phase 3 decides what period a date falls in.** Do not try to fix both in one commit — the two questions
have different right answers and merging them is how the lossy `RequiredCount` happened.

## Test plan

DSL and round-trip (`Wombat.Domain.Tests`, `Wombat.Infrastructure.Tests`):

- `observation_date_field` parses; `Serialize` emits it; a Parse→Serialize→Parse round trip preserves it —
  added to `SeedRoundTripTests`, per CLAUDE.md's rule about properties with no `Serialize` half.
- A pointer naming a field that does not exist is refused at publish, naming the key.
- A pointer naming a `datetime` or `text` field is refused.
- Every seed folder either declares a pointer at a real `date` field or declares none — a corpus-wide
  theory, so a new seed cannot quietly forget.

Resolution and stamping (`Wombat.Infrastructure.Tests`):

- declared + parseable → `Declared` with that date; pointer absent → `CreatedOn`; pointer present but the
  key missing from `DataJson` (legal in `Draft` validation mode) → `CreatedOn`; pointer present, value
  unparseable → `CreatedOn`.
- The stamp is recomputed on create, on draft update, and on a transition whose patch changes the date.
- A transition whose patch *cannot* write the date (post-submit, the CPSA case) leaves the stamp unchanged.
- The resolver reads the **pinned** version: an activity pinned to v1 keeps v1's answer after the type
  publishes a v2 that moves the pointer.

Credit — **the case the evidence run could not exercise, and the reason this task exists**
(`Wombat.Application.Tests`, beside `CreditApplierScalePinningTests`):

> `ProgrammeStartDate` 2025-01-01. Curriculum item with `MinimumLevelByStageJson` `{"1":3,"2":5}`.
> Encounter `observed_on` **2025-06-01** (stage 1). Completed and submitted **2026-03-01** (stage 2).
> Assessor rates **3**.
> Expect `CountsSoFar = 1` **and `MinimumLevelReachedCount = 1`** — graded against the stage-1 minimum of
> 3. Assert explicitly that dating it by submission would have produced `MinimumLevelReachedCount = 0`, so
> the test fails if the wiring is ever reverted.

Plus the mirror: an encounter in stage 2 filed in stage 2 is unaffected, and an activity with no declared
date behaves exactly as it does today.

Readers:

- The trajectory plots a point at the declared date, and its `From`/`To` filters select on `ObservedOn` —
  including the boundary case the current UTC conversion gets wrong (an activity created 00:30 SAST on the
  first day of the window).
- The portfolio PDF prints and filters on the declared date; the committee review window and the sampling
  warnings select on it.
- The dashboard and progress pages still compute their displayed minimum from **today's** stage.

Backdating:

- A future date is refused with a field error on `observed_on`.
- A date before `ProgrammeStartDate` is refused.
- A 200-day-old date is accepted and warned about.

Rebuild:

- Replaying after a restamp produces the stage the declared date implies.
- The rebuild is atomic: an induced failure during the replay leaves the pre-existing progress rows intact.

## Browser verification on dev

The evidence run left everything needed in place, so this is directly checkable:

1. Boot. Confirm the refresher republishes the seven pointer-carrying types and the restamper logs one row
   converted.
2. Open Ndlovu's EPA trajectory. Activity 12's point must move from **2026-09-19** to **2026-03-10**, on
   the chart and in the table beneath it.
3. File a new `mini_cex_cpsa` with `observed_on` set to a date in a *different training year* from today,
   complete it, and confirm the credited minimum is the one for the encounter's stage.
4. Try a future date and a date before the programme start; both are refused at the field.
5. Run the rebuild and confirm Ndlovu's progress row is unchanged (both dates are stage 1 — if it changes,
   something else moved and needs explaining before the session closes).

## Related

Files [T118] finding 4 as its own task. **Blocks [T098] phase 3**, which cannot bucket without a date.
Touches [T073]'s per-stage minimum, which is what makes the re-dating consequential. Requires a decision
about, and probably a fix to, [T106] item 12 and `RebuildCurriculumProgressCommand`'s atomicity. Adjacent
to [T100], which owns the progress page's ambiguous minimum line and should say which stage a displayed
minimum belongs to once two are possible.

---

## 🚨 CORRECTION — 2026-09-19 — the restamper was compatibility flailing. Deleted.

This task specified an `ActivityObservationDateRestamper` to give already-stored activities the date
their clinician typed, and agonised over whether it should resolve the pointer from the **pinned**
schema version or the **newest** one that declares it.

**Both the component and the question were a mistake.** They exist only to repair rows that are
regenerable scenario-replay data. CLAUDE.md's standing rule is explicit: *"A backfill that would have to
guess should be replaced by regenerating the data"*, and *"do not spend design effort on … hedges whose
only purpose is preserving existing rows"*. The restamper was exactly that, and the pinned-vs-newest
debate was a compatibility argument dressed up as a design decision.

Removed: `ActivityObservationDateRestamper`, `WombatOptions.RestampActivityObservationDates`, its DI
registration and its `Program.cs` call.

**What remains is the actual feature.** `ActivityService` stamps the encounter date on every write, so
every activity created from now on carries the date its clinician typed. The migration gives existing
rows `CreatedOn` with `ObservedOnSource = CreatedOn`, which is not a repair — it is the honest statement
that nobody told us when those encounters happened. If correct dates are wanted on the twelve dev rows,
**re-run the scenario runbook or file fresh activities**; filing one through the UI took under a minute
during the T118 evidence run.

`ObservedOnSource` stays, and not for the legacy corpus. `reflective_note` and `qi_project` have no
encounter date by their nature and never will, so a permanent way to say *"this date is the filing
timestamp, not a clinical fact"* is a forward-looking requirement, not a migration artefact.

