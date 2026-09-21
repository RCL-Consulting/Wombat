---
id: T121
title: "Multi-source feedback is required by all 15 EPAs and cannot credit a single one"
status: done
priority: P1
created: 2026-09-19
started: 2026-09-21
completed: 2026-09-21
---
# T121 — Multi-source feedback is required by all 15 EPAs and cannot credit a single one

**Status:** open
**Surfaced:** 2026-09-19, mapping what the v11.1 catalogue still cannot do after the T118 evidence run.
**Severity:** High (functional). MSF is the only tool Annexure A names for **every** EPA, and it is the
only one of the fourteen that already exists as a complete, working aggregate and still cannot move a
trainee's progress by one count. For EPA 5 (`CBD, DOPS, MSF`), EPA 10 (`MSF, Direct observation`) and
EPA 11 (`MSF, CBD, Direct observation`) it is one of only two or three instruments the College permits
at all.

## Symptom

Run a campaign end to end — create, invite, open, collect, close, release. Nothing happens to the
trainee's curriculum progress. No `CurriculumItemProgress` row is created or incremented, no EPA is
named anywhere on the campaign, and nothing on the progress page, the dashboard or a committee review
connects the released report to the curriculum it is evidence for. The trainee's progress page reads
exactly as it did before the campaign existed.

This is not a defect inside the MSF feature. MSF works. It is simply not wired to the only credit
engine the product has, and it carries no EPA to wire it to.

## The aggregate as it stands

Domain, `src/Wombat.Domain/MultiSourceFeedback/`:

| Entity | What it holds |
|--------|---------------|
| `MsfTemplate` | `Name`, optional `SpecialityId`, `AllowPatientResponses`, `IsActive`, questions (`MsfTemplate.cs:3-13`) |
| `MsfQuestion` | `Order`, `Prompt`, `Type` ∈ {`Scale`, `LongText`}, **nullable** `ScaleId`, `Required` (`MsfQuestion.cs:3-12`) |
| `MsfCampaign` | `SubjectUserId`, `TemplateId`, `CreatedByUserId`, `OpensOn`/`ClosesOn` (`DateOnly`), `MinimumResponses` = 8, `MinimumCategoryResponses` = 3, `State`, `CoordinatorNarrative`, `ReviewedByUserId` (`MsfCampaign.cs:3-24`) |
| `MsfInvitation` | `RespondentEmail` → `RespondentEmailHash` on anonymisation, `RespondentCategory`, `TokenHash`, `ExpiresOn`, `RespondedOn`, `RevokedOn`, `AnonymizedOn` (`MsfInvitation.cs:3-18`) |
| `MsfResponse` / `MsfResponseAnswer` | one row per returned questionnaire; each answer is a bare `int? ScaleValue` or a `string? LongText` (`MsfResponseAnswer.cs:3-10`) |

The lifecycle is a hand-rolled state machine on the aggregate: `Open` (Draft → Open), `Close`
(Open/UnderReview → **UnderReview**, not `Closed`), `Release` (UnderReview → Released, requires a
reviewer id), `Withdraw` (anything except Released) — `MsfCampaign.cs:26-70`. Note
`MsfCampaignState.Closed = 2` is unreachable: `Close` sets `UnderReview` (`MsfCampaign.cs:44`). Dead
enum member; harmless, but do not write code that waits for it.

Application, `src/Wombat.Application/Features/MultiSourceFeedback/`:

- `CreateMsfCampaign.cs:40-74` — creates a Draft. Resolves nothing about the subject: no trainee
  profile lookup, no institution, no curriculum. The subject is a raw user id typed into a text box
  (`CampaignEdit.razor:53-55`).
- `AddMsfInvitation.cs:101-131` — draft-only; refuses `Patient` unless the template allows it.
- `OpenMsfCampaign.cs:37-80` — requires ≥1 invitation, regenerates each token, mails the link.
  Responses are anonymous and token-authenticated (`SubmitMsfResponse.cs:45-68`, via the API endpoint
  `src/Wombat.Api/Endpoints/MsfRespond.cs`); the respondent never signs in.
- `CloseMsfCampaign.cs:25-28` — closes and immediately anonymises every invitation email.
  `MsfCampaignAutoCloseJob` does the same hourly for campaigns past `ClosesOn`.
- `ReleaseMsfCampaign.cs:44-51` — **refuses to release until `ReadyForRelease`**, which is
  `Responses.Count >= MinimumResponses` (`MsfAggregationService.cs:80`). Records the reviewer and an
  optional narrative, which is `[Redact]`ed out of the audit summary.
- `MsfAggregationService.cs:12-82` — the only place MSF data is ever reduced. Responses are grouped
  by `RespondentCategory` (`:21`); a category below `MinimumCategoryResponses` is **suppressed
  entirely** — `IsSuppressed = true` and no question data (`:26-29`). For each surviving
  (category, question) pair a `Scale` question yields `MsfScaleAggregateDto(Average, ResponseCount,
  Distribution)` where `Average` is a `double` (`:50-53`).

Read that last line twice, because it decides *What rating an MSF produces* below: **there is no
campaign-level number anywhere in the product.** Not per question, not overall. The only scalars MSF
produces are per-category means of unpinned integers.

## Why it cannot enter the credit path — confirmed

`ICreditApplier` has exactly one method:

```csharp
Task<CreditApplicationResult> ApplyAsync(Activity completedActivity, ActivityType activityType, CancellationToken ct = default);
```
`src/Wombat.Application/Features/Activities/Services/ICreditApplier.cs:34-37`

It reads three things MSF does not have: `Activity.DataJson` (`CreditApplier.cs:39`),
`ActivityType.CreditRulesJson` (`:33`) and the activity's **pinned** `SchemaJson`, from which the
entrustment ladder is resolved (`:70`, `:174-222`). There is no overload and no second entry point:
`ICreditApplier` appears in five places in the whole solution — its own interface file,
`CreditApplier.cs`, `ActivityService.cs:232`, `RebuildCurriculumProgressCommand.cs:59` and the DI
registration at `src/Wombat.Infrastructure/DependencyInjection.cs:104`. Nothing under
`Features/MultiSourceFeedback/` references it, and `MsfCampaign` carries no `EpaId`,
`CurriculumItemId`, `InstitutionId` or `CurriculumId` to credit against even if it did.

Credit is applied at exactly one moment: a transition whose target state is terminal, on a type whose
`counts_for` is non-empty — `ActivityService.cs:212-244`. Two consequences an implementer must know
before designing anything:

1. **`CreateDraftAsync` never credits.** A type whose *initial* state is terminal (the
   `procedure_log` / `journal_club` shape, noted at `ActivityService.cs:96-98`) can never credit
   anything, because the applier is only reached from `TransitionAsync`. Nobody has noticed because
   both of those types declare `"counts_for": []`.
2. **The credit key is built from the activity id**, `$"{activity.Id}:{transition.TransitionKey}"`
   (`CreditApplier.cs:395-404`), and de-duplication is per progress row against that key
   (`:115-119`). Anything that credits without a persisted activity id shares the key `0:…` and
   silently credits once for a whole batch.

## Options

### (a) An activity type that wraps a campaign

MSF becomes an `ActivityType` whose workflow completes when the campaign closes, carrying `epa_id`
like any other tool, with the campaign hanging off the activity.

Cost: the campaign is not expressible as form data. Invitations, single-use token hashes, respondent
categories, the anonymisation sweep and the minimum-category rule are not fields, and the schema DSL
has no way to describe them. So the activity becomes a shell holding a campaign id, driven from
outside — which is option (c) wearing a different hat, plus a false claim that the campaign lives
"inside" the activity. It also leaves the hard question untouched: what does the aggregate put in
`minimum_level_field`?

### (b) `EpaId` on the campaign plus a second credit entry point

Add `MsfCampaign.EpaId` and give `CreditApplier` an overload taking a campaign.

Smallest data change, and the worst structural one. It forks the single credit path — the thing T101
and T109 both spent their length insisting must not happen — and each fork then needs its own copy of:
the scale resolution (`CreditApplier.cs:174-244`), the stage resolution
(`ResolveTraineeAsync` / `GetStage`, `:351-378`), the dedupe key namespace (`:395-404`), the T108
transition stamp (`ActivityTransition.CreditedItemCount`, which has nowhere to land when there is no
transition) and a second branch inside `RebuildCurriculumProgressCommand`, which today iterates
activities and nothing else (`:34-69`). Every future correctness fix must then be made twice, and the
second copy is the one that will be forgotten. Rejected.

### (c) A synthetic activity created when the campaign is released — **recommended**

The campaign stays what it is: the instrument that collects and anonymises feedback. At the moment a
reviewer releases it, the app creates real `Activity` rows of a seeded MSF activity type and
transitions them to a terminal state. Credit then flows down the existing path with **no change to
the credit engine at all**.

It wins on the two things this codebase has already paid for:

- **T109's scale pinning comes free.** The achieved ordinal's ladder is resolved from the pinned
  schema's `scale_key` (`CreditApplier.cs:67-70`, `:200-204`). A seeded `msf_cpsa` whose level field
  declares `scale_key: "CPSA Paediatric Entrustment Scale v11.1"` is scale-pinned exactly like a
  Mini-CEX, so a trainee on a five-rung curriculum gets the `ScaleMismatch` refusal rather than a
  silent mis-credit. Option (b) would have to re-derive the ladder from `MsfQuestion.ScaleId` — which
  is nullable, and is `null` for every template the product can actually create
  (`CampaignEdit.razor:144-151` passes `null`).
- **T108's narrowing stays enforceable.** T108 narrowed the EPA *picker* to EPAs creditable on the
  subject's curriculum. A synthetic activity bypasses every picker, so the fan-out must apply the same
  predicate server-side — which is easy, because the predicate already exists in the engine
  (`CreditApplier.ResolveCurriculumItemsAsync:249-286`: items on the trainee's curriculum where
  `OwningInstitutionId` is null or theirs). Do that and MSF cannot re-open the hole T108 closed.

And one more: `RebuildCurriculumProgressCommand` replays terminal activities (`:34-57`). Synthetic
activities are ordinary activities, so the rebuild re-credits MSF correctly with **zero** lines
changed.

## Chosen design — option (c)

### The seed

A new seed folder `src/Wombat.Infrastructure/Activities/Seeds/msf_cpsa/`, registered in
`ActivityTypeSeedCatalogue.Entries` (`ActivityTypeSeedCatalogue.cs:99-111`) as
`ActivityTypeSeedSource.PaediatricCollege`, `ActivityScope.Speciality`, `DisplayFieldsRule.None`.
Registration is not optional: an unregistered folder is invisible to the T103 refresher, so later
edits to it would be inert on any database that has already booted (CLAUDE.md, *Editing a seed
folder*). The `_cpsa` suffix is not optional either — the reasoning is at
`PaediatricCatalogueSeeder.cs:79-90`.

`schema.json`, one section, every field owned by the creator:

| Field | Type | Required | Why |
|-------|------|----------|-----|
| `epa_id` | `epa` | yes | what `curriculum_item_match.epa_field` reads |
| `campaign_id` | `number` | yes | provenance: which campaign this row is a face of |
| `observed_on` | `date` | yes | set to `campaign.ClosesOn` — the date the evidence was complete |
| `respondent_count` | `number` | yes | how many questionnaires stood behind it |
| `overall_level` | `scale` | **no** | `scale_key: "CPSA Paediatric Entrustment Scale v11.1"`, `validation.min 1 / max 6` |
| `summary` | `longtext` | no | the coordinator narrative, copied verbatim |

**Nothing respondent-identifying goes into `DataJson`, ever.** No emails, no per-respondent comments,
no per-category breakdown. `AccessReportBuilder` puts an activity's whole `DataJson` into the
subject's data-subject access report (see T112), and MSF only works because it is confidential and
aggregated. The narrative is safe because it is already released to the trainee
(`ListMsfCampaignsForTrainee.cs:26` returns Released campaigns to them).

`workflow.json`: `draft → recorded (terminal)`, a single transition `record`, and its `actor` is
**`role:Coordinator|role:Administrator`**, not `creator`. That is the gate that stops a trainee
hand-filing an `msf_cpsa` from `/activities/new` and crediting themselves: `ListActivityTypesQuery`
offers every published speciality-scoped type to every member of that speciality
(`ListActivityTypes/ListActivityTypesQuery.cs:28-38`), and there is no "system-managed, not
hand-creatable" concept anywhere — `ActivityPermissionRule` exists as a table and a DbSet and is read
by nothing. A trainee can therefore create a stray draft; with this actor rule they can never
complete it, so it can never credit. The clutter is cosmetic, and it is the same complaint T118
findings 6 and 7 already make about the picker; a `SystemManaged` flag on `ActivityType` is the
durable answer and belongs with that work, not here.

`credit.json`: **superseded by D8, which was answered after this paragraph was written.** The rule below
is what the four seeded CPSA tools carry and is what this design proposed; the College's answer to D8 is
that MSF consumes none of Annexure A's 55 encounters, so the seed ships `{ "counts_for": [] }` — the
`procedure_log` shape — and nothing in `CreditApplier` is ever reached. Left here because everything
below about `minimum_level_field` and `ValueMissing` is the reasoning D8 was decided against.

```json
{ "counts_for": [ { "curriculum_item_match": { "epa_field": "epa_id" }, "amount": 1, "minimum_level_field": "overall_level" } ] }
```

### Where it fires: release, not close

`ReleaseMsfCampaignCommandHandler.Handle` (`ReleaseMsfCampaign.cs:42-52`), after
`campaign.Release(...)`.

Not close, for a reason that will otherwise be found the hard way: `Close` moves the campaign to
`UnderReview` (`MsfCampaign.cs:44`) and `Withdraw` is legal from `UnderReview`
(`MsfCampaign.cs:61-70`), so crediting at close leaves credit standing behind a withdrawn campaign.
Release is the point of no return — `Withdraw` refuses a Released campaign (`:63-66`) and `Release`
itself refuses unless the state is `UnderReview` (`:50-53`), so the handler is once-only by
construction.

Release already refuses below `MinimumResponses` (`ReleaseMsfCampaign.cs:45-48`), so the credit gate
inherits a real evidential threshold for free. Note what that means operationally: a campaign that
never reaches 8 responses can never be released and therefore never credits, and
`MsfCampaignAutoCloseJob` will happily park it in `UnderReview` for ever. That is correct behaviour,
and it is why the broken reminder link in *Adjacent defects* sits on this task's critical path.

### The fan-out: one activity per covered EPA

The campaign declares which EPAs it is evidence for, explicitly, as data:

- `MsfCampaignEpa` — a join row `(CampaignId, EpaId)`, unique on the pair. One new table, one
  migration (hand-written plus its `.Designer.cs` and a snapshot update, per CLAUDE.md, if `dotnet
  ef` cannot reach a database).
- Chosen at campaign creation by the coordinator, from the subject's own curriculum: resolve the
  `TraineeProfile.CurriculumId` for `SubjectUserId` and offer that curriculum's items' EPAs. This is
  T108's predicate applied at the only picker MSF has.
- Re-validated at release rather than trusted from creation: a trainee may have been moved between
  curricula in between (Ndlovu was, in T118). Any EPA no longer on the subject's curriculum is
  dropped with a log line, not an exception — a release must not fail because an administrator moved
  someone.

One activity per covered EPA, rather than one activity naming many, because `curriculum_item_match`
reads **one** integer out of `DataJson` (`CreditApplier.cs:269-283`). A list would need a new match
rule, which is a DSL change carrying the Parse-and-`Serialize` trap CLAUDE.md warns about plus a
`SeedRoundTripTests` entry — for no gain. Per-EPA rows also give the CCC what it wants: each EPA's
evidence standing on its own, individually sampled.

The cost is real and should be stated to the maintainer: a campaign covering 8 EPAs puts 8
near-identical rows in the trainee's activity list. They share a `campaign_id` and differ only by
EPA, so a grouped display is the eventual fix; it is not this task's.

### Persisting it: two saves, and why

`IApplicationDbContext` exposes only `SaveChangesAsync` and `Set<T>`
(`src/Wombat.Application/Common/Interfaces/IApplicationDbContext.cs:5-9`), so an Application handler
cannot open a transaction, and `CreateDraftAsync` / `TransitionAsync` each save internally
(`ActivityService.cs:122`, `:248`). Calling them in a loop would commit the release on the first save
and then risk a half-finished fan-out.

Add one method to `IActivityService`, implemented in Infrastructure. (The reason given here originally —
"where the real `DbContext` lives" — was wrong: `ActivityService` holds only `IApplicationDbContext` and
cannot open a transaction either. The real reasons are the credit-key argument below and access to the
internal `ActorRuleMatcher`/`ObservationDateResolver`.)

```csharp
Task<IReadOnlyList<ActivityDto>> RecordCompletedAsync(RecordCompletedActivitiesInput input, CancellationToken ct = default);

public sealed record RecordCompletedActivitiesInput(
    string ActivityTypeKey,                 // "msf_cpsa"
    string SubjectUserId,
    string CreatedByUserId,                 // the releasing reviewer
    string TransitionKey,                   // "record"
    IReadOnlyList<string> DataJsonPerActivity,
    ClaimsPrincipal Principal);
```

It reuses everything: `ResolveSubjectScopeAsync` for the T101 institution/speciality/sub-speciality
stamp, `FieldPermissionEvaluator` for the writable set, `SchemaValidator`,
`Activity.ApplyTransition`, and `ICreditApplier.ApplyAsync` with the pinned version's
`CreditRulesJson` **and** `SchemaJson` — the same pair `ActivityService.cs:232-241` passes, because
the schema is what binds the ordinal to a ladder.

It saves **twice**, and this is not negotiable: the credit key is `"{activity.Id}:{transitionKey}"`
(`CreditApplier.cs:395-404`) and an unsaved activity has `Id = 0`, so every row in the batch would
share the key `0:record` and only the first would ever credit an item. So: save once to assign ids,
then apply credit per activity and let the caller's existing `SaveChangesAsync`
(`ReleaseMsfCampaign.cs:51`) commit the rest.

A crash between the two saves leaves completed activities with no credit and
`ActivityTransition.CreditedItemCount` null — which is precisely T108's three-valued contract for
"credit was never evaluated", and is repairable by `RebuildCurriculumProgress`. To stop a retry
creating a second set of activities, add `MsfCampaign.CreditedOn` (nullable `DateTime`), set by the
handler before the call so it commits with save 1; the handler skips the fan-out when it is non-null.

`ReleaseMsfCampaignCommand` must gain `ClaimsPrincipal Principal` — `CreateActivityInput` and the
field-permission evaluator require one (`Features/Activities/Dtos/ActivityInputs.cs:5-10`) — which is
the T056 convention the whole MSF folder has never adopted anyway (see *Adjacent defects*).

## What rating an MSF produces — settled

Check this first, because the answer decides everything else: **the MSF questionnaire does not produce
a scale rating today, in any usable sense.**

- A `Scale` question's `ScaleId` is nullable (`MsfQuestion.cs:10`) and the only surface that creates a
  template passes `null` (`CampaignEdit.razor:144-151`). The values are unpinned integers.
- `MsfCampaignRules.ValidateResponsePayload` checks that a scale answer is *present*
  (`MsfCampaignRules.cs:81`) and nothing else. There is **no range check** — not against the
  question's scale, not against anything.
- The aggregate is a per-(category, question) arithmetic **mean** as a `double`
  (`MsfAggregationService.cs:50-53`). On the CPSA ladder — rungs `1, 2, 3a, 3b, 4, 5` at ordinals
  1–6 — a mean of 3.4 is not a rung, and the rungs it sits between are not evenly spaced. There is no
  campaign-level scalar at all.

And here is the trap that makes "just credit volume, skip the level" wrong if done naively: a
directive with neither `minimum_level_field` nor `minimum_level_fixed` returns
`EntrustmentLevelComparer.NotGated()` (`CreditApplier.cs:295-299`), which is
`(MinimumMet: true, Basis: NoGate)` (`src/Wombat.Domain/Epas/EntrustmentLevelComparison.cs:43`) — so
it increments **`MinimumLevelReachedCount`** (`CreditApplier.cs:122-125`). An ungated MSF would
record the trainee as having met the supervision minimum on every EPA it touched, on the strength of
a questionnaire that never asked about supervision. That is T109's defect in a new costume: silently
*wrong* credit, not silently absent.

**Settled, then superseded in one half by D8:** the field is **optional**, and that half stands. The seed
does **not** declare `minimum_level_field`, because it declares no credit directive at all. What survives
is `rated_level_field: "overall_level"` at the schema root (T126), which binds the ordinal to the CPSA
ladder without asserting any credit.

- When the releasing reviewer records an ordinal, it gates exactly like a Mini-CEX rating — same
  comparer, same scale pinning, same refusal across ladders.
- When they do not, `CompareMinimumLevel` falls through to
  `new LevelComparison(false, LevelComparisonBasis.ValueMissing)` (`CreditApplier.cs:321`), which
  increments `CountsSoFar`, leaves `MinimumLevelReachedCount` alone, and touches none of T109's three
  anomaly counters — the switch at `:131-149` has no `ValueMissing` case. That is exactly "counts as
  evidence, asserts no level", and it needs **no engine change and no DSL change**.

This leans on `ValueMissing`, which T109's table documents as a degenerate case ("gated, but no
readable ordinal") rather than a designed one. Nothing in the product can currently produce the
degenerate version — every seeded tool lists its level field in its completing transition's
`requires_fields` (`mini_cex_cpsa/workflow.json`) — so the ambiguity is theoretical today. It will
stop being theoretical when the other non-entrustment annexure tools land: reflective exercise,
clinical audit, learner feedback, and portfolio and logbook review, four of the fourteen. At that
point add a real `minimum_level: "not_assessed"` to the credit grammar and a matching
`LevelComparisonBasis`, so the engine says what it means. **Do not do that now** — it is a
Parse+`Serialize` DSL change for a second caller that does not yet exist.

Who supplies the ordinal is a clinical question, not a code one, and it is D3 below.

## 🚨 Decisions — state them, do not let an implementer invent them


> **Renumbered 2026-09-20.** These four were locally numbered D1–D4, which collided head-on
> with the product register: this task's D1 was the College's **D9**, D2 was **D8**, D3 was **D10**,
> D4 was **D11**. Same letter, two unrelated sequences, no way to tell from a citation which
> register you were in — the exact trap `DECISIONS.md` splits `W-nnn` from `D-n` to avoid. They now
> carry the College's numbers, and all four are answered. See `EPA-PROGRAMME.md` § 3A-ii.

### D9 — Is MSF run once per period covering many EPAs, or once per EPA? — **ANSWERED: per period**

The design above is deliberately neutral: `MsfCampaignEpa` expresses either, and the decision changes
guidance and coordinator workload, not schema. But the arithmetic is one-sided.

`MinimumResponses` defaults to 8 and `MinimumCategoryResponses` to 3, across six respondent
categories (`MsfCampaign.cs:12-13`, `MsfRespondentCategory.cs`), and release is refused below the
total (`ReleaseMsfCampaign.cs:45-48`).

- **Per EPA:** 15 EPAs × 8 completed questionnaires = **120 returned questionnaires per registrar per
  year**, each campaign needing ≥3 responses in each category that is to be reported. At a generous
  60% response rate that is ~200 invitations per registrar per year. A department with 20 registrars
  issues ~4,000 invitations a year onto the same ~30 consultants and ward nurses: roughly **80
  questionnaires per respondent per year**, one every four working days. Response rates collapse,
  `ReadyForRelease` never trips, and nothing credits at all.
- **Per period (semester), covering many EPAs:** 2 × 8 = 16 questionnaires per registrar per year,
  ~26 invitations; ~520 per department per year, ~16 per respondent. That is what departments
  actually run, and it matches Annexure A's `status` line for EPA 1: *"Essential — each semester (six
  months)"*.

I would choose per-period and would not build the per-EPA variant at all. But it changes what "MSF on
EPA 7" means on a certificate, so the College should say it.

Two facts to weigh alongside: `CurriculumItemProgress` is one row per (item, trainee) **for life**
(T098 gap 1), and the credit grammar's `amount` is applied per matched curriculum item per activity
(`CreditApplier.cs:121`) — so per-period campaigns still produce per-item counts, and neither answer
needs a schema change beyond the join table.

### D8 — Does MSF count toward the 55 observed encounters per year? — **ANSWERED: no**

Annexure A's frequencies sum to exactly 55 (6+6+6+6+6+4+2+1+1+6+1+6+1+1+2), so the 55 **is** the sum
of the per-EPA quotas — the same number `RequiredCount` encodes, multiplied by four programme years
at `PaediatricCatalogueSeeder.cs:361`. So this is not an abstract question. If an MSF credits 1
against each covered EPA, two semester campaigns supply **30 of the 55 encounters from two
questionnaires**, and for the EPAs whose quota is "One per annum" (8, 9, 11, 13, 14) a single MSF
discharges the entire year's requirement — EPA 14 (`MSF, Reflective exercise, CBD`) would be complete
for the year without the trainee doing anything EPA-specific at all. The counter is then wrong from
day one.

Also relevant: Annexure A's `tools` field is **free text** — an allow-list with a count of the listed
tools in parentheses, not a quota. There is no per-tool sub-quota anywhere in the source, so any cap
("at most one of EPA 1's six may be an MSF") is an invention and must be a decision, not an
inference.

Three realistic answers:

1. **Yes, uncapped** — `amount: 1` as designed above. Simplest; inflates the counter as described.
2. **Yes, capped at one per period per EPA** — needs the period key from T098 phase 3 and a per-tool
   counter. Not expressible today; out of scope until phase 3 lands.
3. **No** — MSF is required evidence tracked in its own right, not part of the 55. Implementable
   today with a one-line seed edit (`"counts_for": []`, the `procedure_log` shape), which leaves this
   task's synthetic activities carrying the evidence, the provenance and the optional rating while
   contributing nothing to the quota. MSF's own requirement then rides on phase 3's tool-mix counter.

I would choose 3 now and 2 when phase 3 exists. That keeps the 55 meaning what the College says it
means, and it makes this task's value the *evidence link*, not the count.

### D10 — Who states the entrustment level an MSF asserts, if anyone? — **ANSWERED: the releasing reviewer**

The mechanism is settled above (an optional `overall_level`). Who fills it in is not:

- **The respondents.** Add a scale question bound to the curriculum's entrustment scale, ask every
  respondent the supervision question directly, and define the campaign ordinal as the **median**
  (never the mean — a mean is not a rung). Respondents include nurses, allied health professionals
  and patients, who are not positioned to judge entrustment. That is a College question, not a
  product one.
- **The releasing reviewer.** One ordinal recorded beside the narrative they already write
  (`ReleaseMsfCampaign.cs:50`), informed by the aggregate. A named clinician stands behind it, which
  is how every other rating in the product works.
- **Nobody.** Leave it blank always; MSF credits volume and asserts nothing.

I would choose the reviewer, with "nobody" as a perfectly acceptable default — the design supports
both with no code difference, because the field is optional either way. The respondent option is the
only one that needs new questionnaire design and College sign-off.

### D11 — Does a campaign with suppressed categories still credit? — **ANSWERED: needs two surviving categories**

`ReadyForRelease` counts total responses only (`MsfAggregationService.cs:80`). The per-category
minimum is enforced *nowhere*: it only suppresses a category's data from the report (`:26-29`). So a
campaign answered by eight peer doctors releases, shows one category and five suppressed ones, and
under this design would credit. If the College's view is that MSF means *multi*-source, then "at
least N categories survive suppression" should join the release gate. It is one predicate in
`ReleaseMsfCampaignCommandHandler`; I would add it, but it changes when existing campaigns can be
released, so it is the maintainer's call.

## Implementation order

1. `MsfCampaignEpa` join entity, its configuration, and a migration (plus `.Designer.cs` and snapshot
   update). `MsfCampaign.EvidenceRecordedOn` in the same migration.
2. EPA selection on `CampaignEdit.razor`, sourced from the subject's curriculum. Replace the
   free-text subject user id while you are in there.
3. The `msf_cpsa` seed folder and its `ActivityTypeSeedCatalogue` registration. No `SeedRoundTripTests`
   entry is needed — its theories enumerate seed folders from disk. `SeedScaleKeyTests`'s
   `ExactlyEightSeededToolsAreRated` is the one that has to change, and changing it is the decision
   that `msf_cpsa` counts as rated evidence.
4. `IActivityService.RecordCompletedAsync` and its Infrastructure implementation.
5. `ReleaseMsfCampaignCommand` gains `ClaimsPrincipal Principal`; the handler re-validates EPA
   coverage, sets `EvidenceRecordedOn`, and calls `RecordCompletedAsync`.
6. Surface it: the released campaign report names the EPAs it is evidence for, and the committee
   snapshot's campaign row names its per-EPA children.

## As built — where the shipped code differs from the design above

The design was written 2026-09-19, before the College answered D8/D9/D10/D11 and before T126, T133 and
T134 landed. Five deliberate divergences:

1. **`counts_for: []` (D8).** Nothing is credited, so no `CurriculumItemProgress` row moves and
   `ActivityTransition.CreditedItemCount` stays null — T108's "credit was never evaluated". The value of
   the task is the evidence link, not a count. The Verification list below is rewritten accordingly.
2. **`EvidenceRecordedOn`, not `CreditedOn`.** Naming a column "Credited" when D8 says MSF credits
   nothing would mislead every future reader. It is also not a duplicate of `ReleasedOn`: a release whose
   fan-out produced nothing leaves it null while `ReleasedOn` is set, which is the queryable repairable
   state.
3. **`rated_level_field` is declared, and that puts `msf_cpsa` in the rated set.**
   `SeedRoundTripTests.Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale` makes it a biconditional: a
   schema carrying a `scale` field MUST name its rated field. Keeping D10's optional ordinal therefore
   means joining the rated set; the only escape would be deleting the field, which deletes D10. The
   consequences are that the committee sampling report and the trajectory chart both consider the row and
   both then skip it for want of an `assessor_user_id` — which is the right answer (an MSF asserts a level
   but has no observing assessor to concentrate or to plot), reached by an accidental mechanism. Pinned by
   `SeedScaleKeyTests.ExactlyNineSeededToolsAreRated` so it is a decision rather than a drift.
4. **Both schema sections declare `editable_by: role:Coordinator|role:Administrator`.** The design left
   the sections default (`subject|creator`), which would have let a trainee fill in the reviewer's
   entrustment level and narrative on their own stray draft. They can still create the draft — nothing in
   the product expresses "system-managed" — but every field they submit is now dropped at creation and the
   draft is permanently inert.
5. **`CreateMsfCampaignCommand` also gained a `ClaimsPrincipal` and an institution check**, which the
   design listed as adjacent defect 3 and left alone. Made necessary here: a cross-institution campaign
   used to be an inert row, and now fans out activities scope-stamped from the subject's profile.

Two smaller ones: the D11 gate lives in `MsfAggregationService.BuildReport` rather than in the release
handler, so the disabled button and the server refusal cannot disagree; and `MinimumRespondentCategories`
is a campaign column beside the other two thresholds rather than a constant.

### And six things an adversarial review of the diff found, all fixed

Six independent reviewers read the working tree; three verifiers tried to refute each finding. What
survived, and what it changed:

1. **The release, not the create, is where the scope check belongs.** The first pass gated
   `CreateMsfCampaignCommand` and left `ReleaseMsfCampaignCommand` open — and release is the half that
   writes. Neither `ListMsfCampaignsForCoordinatorQuery` nor `GetCampaignAggregateReportQuery` filters
   by principal, so any Coordinator could reach any campaign's report page by id and release it, planting
   permanent evidence in another institution's trainee's portfolio. The rule now lives once, in
   `MsfCampaignRules.EnsureSubjectIsInScopeAsync`, and both handlers call it.
2. **A failed fan-out would have committed the release.** `AuditWriter` shares the request's scoped
   `IApplicationDbContext` and calls `SaveChangesAsync`, and `AuditPipelineBehavior` writes an audit row
   from its `catch` — so any exception raised while `State = Released` was pending flushed it on the way
   out, and `Release` refuses a second attempt. The handler now does every throwable thing first;
   `StageCompletedAsync` validates the whole batch before adding anything and **does not save at all**,
   so the release and its evidence commit together or not at all. Asserted by
   `AFailedReleaseLeavesTheCampaignUnreleasedAndRetryable`.
3. **The reviewer's ordinal was a bare number box** — the T100 trap exactly. On the CPSA ladder order 5
   is rung "4", so a reviewer who meant "4" and typed 4 stored rung "3b", and rungs "3a"/"3b" could not
   be entered at all. It is now a `<select>` of the College's rungs, resolved from the type's own
   `rated_level_field` scale. **Browser-verified: choosing "4" stores 5 and renders back as "4".**
4. **The type's speciality scope was not checked.** The MSF trainee picker is scoped by institution, not
   speciality, so an institution running Paediatrics alongside another discipline could stamp `msf_cpsa`
   records — carrying an ordinal pinned to the CPSA ladder — onto a non-paediatric registrar. D8's empty
   `counts_for` removes `CreditApplier`'s `ScaleMismatch` refusal, which would otherwise have caught it.
   `StageCompletedAsync` now refuses a scoped type's record about an out-of-scope subject.
5. **The evidence was dated from the SCHEDULED close.** A campaign closed early was dated in the future,
   which would put it outside its own committee review window and into the wrong period for [T130].
   It now uses `ClosedOn`. **Browser-verified**: a campaign scheduled to close 2026-10-21 and closed on
   2026-09-21 produced evidence dated 2026-09-21.
6. **A dropped EPA was still reported as recorded.** `MsfCampaignEpa.RecordedOn` now marks the ones that
   actually produced an activity, so the committee snapshot, the report page and the portfolio PDF say
   which EPAs were recorded and which were declared and dropped, rather than claiming all of them.

One more, taken because the fix is a line and the claim is otherwise false: **two concurrent releases
would have written two full sets of evidence.** The once-only guarantee rested on `Release` refusing
anything but `UnderReview`, which is an in-memory check on a row both requests read as `UnderReview`.
`MsfCampaigns` now carries Postgres's `xmin` as a concurrency token — no column, no write path.

## Verification

- [x] A released campaign covering 3 EPAs creates 3 terminal `msf_cpsa` activities, one per EPA, each
      carrying `epa_id`, `campaign_id`, `observed_on` = the campaign's `ClosesOn`, `respondent_count`, and
      the reviewer's optional `overall_level` and `summary` — **browser-verified on dev 2026-09-21**,
      campaign 1 covering PAED-001/010/012, activities 1–3 in `recorded`.
- [x] **No** `CurriculumItemProgress` row moves and `CreditedItemCount` stays null on every transition
      (D8) — browser-verified (0 progress rows, 6/6 null stamps) and pinned by
      `MsfEvidenceFanOutTests.Release_CreditsNothing_BecauseMsfConsumesNoneOfTheFiftyFiveEncounters`.
- [x] `Activity.ObservedOn` is the day the window ACTUALLY shut, not the release date and not the
      scheduled close (T119) — browser-verified on a campaign scheduled to close 2026-10-21 and closed
      on 2026-09-21: the evidence is dated 2026-09-21.
- [x] The reviewer's ordinal is chosen as a College rung, not typed as an order — browser-verified:
      selecting "4" stores 5 and the activity renders it back as "4" (T100).
- [x] A release that throws leaves the campaign releasable, despite the audit pipeline's
      `SaveChangesAsync` on the same scoped context — `AFailedReleaseLeavesTheCampaignUnreleasedAndRetryable`.
- [x] Release is refused for a trainee outside the caller's institution, and for a trainee outside the
      type's speciality — `Release_RefusesACampaignAboutAnotherInstitutionsTrainee` and
      `Release_RefusesToWriteAPaediatricRecordAboutATraineeOfAnotherSpeciality`.
- [x] A dropped EPA is reported as declared-but-not-recorded everywhere it appears — browser-verified on
      the report page, and pinned by `StartReview_MsfEvidenceNamesTheEpasItStandsBehind`.
- [x] The activity is scope-stamped from the SUBJECT's profile, not the releasing coordinator's (T101) —
      verified institution 1 / speciality 2 / sub-speciality 2 on all three rows.
- [x] An EPA that has left the subject's curriculum is dropped with a log line and the release still
      succeeds — `Release_DropsAnEpaThatHasLeftTheSubjectsCurriculum_WithoutFailingTheRelease`.
- [x] Release is refused until at least `MinimumRespondentCategories` categories survive suppression
      (D11), and the button says which gate is closed — browser-verified (button disabled, reason shown)
      and pinned by `Release_IsRefused_WhenOnlyOneRespondentCategoryReports`.
- [x] Releasing twice is impossible (`MsfCampaign.Release` throws) and a replay with `EvidenceRecordedOn`
      set creates no second set of activities — `Release_DoesNotRepeatTheFanOut_WhenEvidenceIsAlreadyRecorded`.
- [x] A trainee cannot complete a hand-created `msf_cpsa`, and every field they submit is dropped at
      creation — `ATraineeCannotRecordAHandCreatedMsfActivity`.
- [x] The stored `DataJson` contains no respondent email, no per-respondent comment and no per-category
      breakdown — `TheEvidencePayloadCarriesNothingThatIdentifiesARespondent` and
      `MsfSeedTests.ItCarriesNoFieldThatCouldIdentifyARespondent`, which pins the whole field list.
- [x] The EPA picker offers exactly the subject's curriculum EPAs (T108's predicate) — browser-verified:
      15 PAED EPAs for a paediatric registrar.
- [x] The migration has its `.Designer.cs` and the snapshot is updated, and `database update` applies it —
      applied to the dev database 2026-09-21.
- [x] Nothing that credits today changes — all five suites green, 1040 tests.
- [ ] `RebuildCurriculumProgress` reproduces the same rows with no code change. **Vacuous under D8**: the
      rebuild skips `msf_cpsa` at its own `DeclaresCredit` gate, because there is nothing to re-credit.
      Left unticked rather than deleted, because it becomes real the day MSF gains a `counts_for`.

## Adjacent defects found while mapping this — recorded here, not fixed here

Each is real, each is in the MSF feature, and none is this task's subject. They are written down so
they are not rediscovered a fourth time.

1. **The expiry reminder emails a dead link.** `MsfInvitationExpiryReminderJob.cs:57` builds
   `/msf/respond?token={invitation.TokenHash}` — the **hash**, not the token. `VerifyToken` hashes
   what it is given and compares (`InvitationTokenService.cs:32-51`), so a hashed hash never matches.
   Raw tokens are generated and discarded at open (`OpenMsfCampaign.cs:55-56`) and are never stored,
   so this job **cannot** send a working link without a redesign (re-issue a fresh token and re-hash,
   as `OpenMsfCampaign` does). This sits on the critical path for MSF ever reaching
   `MinimumResponses`, and therefore for MSF ever crediting.
2. **`ListMsfCampaignsForCoordinatorQuery` returns every campaign in every institution, unfiltered**
   (`ListMsfCampaignsForCoordinator.cs:21-39`) — no principal, no scope. Same family as T112 and
   T117. Note that this task's synthetic activities *are* scope-stamped from the subject's profile
   (T101), so the credit is correctly scoped even while the campaign list is not.
3. **No MSF command or query takes a `ClaimsPrincipal`.** The folder predates T056 and gates on
   page-level `[Authorize(Roles = "Coordinator,Administrator")]` alone (`CampaignEdit.razor:3`,
   `CampaignReport.razor:2`). Any Coordinator can create a campaign about any user id they can type,
   and release anyone's.
4. **`GetActiveInvitationByTokenAsync` loads every invitation in the database** and linear-scans them
   in memory (`MsfCampaignRules.cs:26-36`), because a token is verified by hash comparison rather
   than looked up. Correct, and it will not scale.
5. **MSF will not appear on the trajectory chart**, deliberately, under this design.
   `TryResolveSource` matches a hardcoded family map with no MSF entry
   (`GetEpaTrajectoryForTraineeQuery.cs:77-90`) and `TryParseObservation` additionally requires an
   `assessor_user_id` (`:229-232`) — for which MSF has no honest value; naming the release reviewer
   "the assessor" would be a lie on a clinician-facing chart. If the maintainer wants MSF plotted, it
   is two changes (a family entry plus relaxing the assessor requirement) and it should be decided
   with T118 finding 8, not here.
6. `MsfCampaignState.Closed` is unreachable (`MsfCampaign.cs:44` sets `UnderReview`), and
   `MsfCampaignAutoCloseJob` carries its own private copy of `AnonymizeInvitations` rather than
   calling `MsfCampaignRules`. Cosmetic; noted so nobody assumes the enum member is a live state.

## Related

The concrete instance of T098 gap 6 ("MSF carries no EPA reference and has **no credit path
whatsoever**"). It answers T098's open question 2 only by raising it properly, as D2 above. It is
blocked by nothing and depends on nothing. It inherits T109's scale pinning and T108's EPA narrowing
by construction rather than by re-implementation. Its `observed_on` is written for the benefit of the
`observed_on` fix (T118 finding 4) and of T098 phase 3's period resolver — whose anchor is now
decided as the fixed academic year subdivided into two semesters, which `TraineeProfile.GetStage`'s
365-day blocks from `ProgrammeStartDate` (`TraineeProfile.cs:66-75`) contradict by construction. This
task must not invent a second period concept: it writes a date and lets phase 3 bucket it.
