---
id: T137
title: A campaign's per-EPA evidence rows are indistinguishable on the trainee's own activity list
status: done
priority: P2
owner: agent
model: opus
depends_on: []
created: 2026-09-21
started: 2026-09-24
completed: 2026-09-24
---

# T137 — Eight identical rows, one per EPA, and nothing on the page says which is which

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Nothing is wrong in the data; the page simply cannot express it. It is the
trainee's own portfolio surface, and a campaign covering eight EPAs makes it unreadable in one act.
**Surfaced:** 2026-09-21, browser-verifying [T121]. The task file predicted "the clutter is cosmetic";
the completeness critic sharpened that correctly to "the rows are not merely numerous, they are
unidentifiable", and the browser run confirmed it.

## Symptom

`/activities/mine` after releasing one MSF campaign covering three EPAs, verbatim:

```
Type                                  State     Updated
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
```

The only way to tell them apart is to open each one. An Annexure A campaign covers up to fifteen.

## Root cause

Not MSF-specific, and not `DisplayFieldsRule.None`: **no type's display fields are rendered on that page
at all.** `ActivitySummaryDto` carries Id, ActivityTypeId, Key, Name, SubjectUserId, CurrentState,
CreatedOn, UpdatedOn (`ListActivitiesBySubjectQuery.cs:44-52`) and `MyActivities.razor:24-33` renders
Type / State / Updated. There is no EPA and no encounter date in the projection.

`Activity.EpaId` exists as a column (`Activity.cs:15`) and is **written by nothing** — grep finds it
assigned nowhere, read only by `ActivityService.Map`. So the row's EPA lives only inside `DataJson`,
which is not SQL-projectable.

## What to build

Two candidate shapes; the second is the durable one.

1. **Render what is already there.** Add `ObservedOn` to `ActivitySummaryDto` and show it. Helps every
   type (T119 made `ObservedOn` the real date and the list still shows the audit clock), but does not
   separate siblings that share an encounter date.
2. **Populate `Activity.EpaId` at write time**, exactly as T119 populates `ObservedOn`: stamp it from
   the `epa`-typed field the pinned schema declares, on create, draft update and transition. Then the
   list can project and join it, and so can everything else that currently re-parses `DataJson`
   (`GetSamplingConcentrationWarnings.TryParseRating`, `GetEpaTrajectoryForTraineeQuery.TryParseObservation`).
   Needs a pointer at the schema root — `epa_field`, the third after `observation_date_field` and
   `rated_level_field` — with **both** a Parse and a Serialize half, or it is dropped silently at publish.

A grouped display ("Multi-Source Feedback — 3 EPAs, campaign #1") is a third option and is cosmetic
only; it does not fix the projection that every other reader also lacks.

## Verification

- [x] A trainee whose released campaign covered three EPAs can tell the three rows apart without
      opening them — checked in the browser
- [x] Whatever is added to `ActivitySummaryDto` is populated for every seeded type, not only MSF —
      checked by a test over the seed corpus
- [x] If `Activity.EpaId` is populated: a DSL property added for it survives Parse+Serialize — checked
      by a named `SeedRoundTripTests` fixture, per CLAUDE.md's Serialize-half trap
- [x] Full suite green — `dotnet test` per project, no `--no-build`

## Related

[T121] created the rows that make this visible. [T118] findings 6 and 7 are the neighbouring picker
complaints. [T119] is the precedent for stamping a column from a schema-declared pointer.

## Notes

- **Observed:** `Activity.EpaId` and `Activity.CurriculumItemId` are both dead columns today.
- **Observed:** the information is one click away on `ActivityView`, which renders every field; this is
  a list-projection gap, not a data loss.

## Update 2026-09-24 — EPA-stream survey

**[T106] item 14 is folded in.** Carry the last crediting transition's `CreditedItemCount` on `ActivitySummaryDto`,
through `ListActivitiesBySubjectQuery`, and show it as a column on `MyActivities`. It is three-valued, per [T108]: a
count, zero ("credited nothing"), or null ("credit never evaluated", shown blank). Same DTO, same row, same change. It
is how a registrar sees across the whole list which completions counted.

**The plan: the survey's recommendations, adopted.**

- **Option 2: stamp `Activity.EpaId` from a schema-root pointer.** The pointer must agree with the credit directive's
  `epa_field` (`CreditRulesParser.cs:108-112`). Either publish refuses a type where the two differ, or a credit
  directive that names no `epa_field` defaults to the pointer. Either way the list and credit can never name different
  EPAs. The pointer needs both a Parse half and a Serialize half, and a named `SeedRoundTripTests` fixture (CLAUDE.md's
  Serialize-half trap). Choose a name that cannot be misread as the credit directive's property, or state plainly
  that the two are the same field.
- **Stamp at all three sites that stamp `ObservedOn`:** create (`ActivityService.cs:188`), transition (`:302`), and
  the system-written path (`:479`). The last is where the MSF rows come from, and is the reason this task exists.
- **Add `ObservedOn` to `ActivitySummaryDto` in the same change (option 1).** It costs almost nothing and replaces the
  audit clock the list shows today for every type. On its own it cannot separate MSF siblings: every row of a campaign
  shares one `observed_on` (`ReleaseMsfCampaign.cs:226-230`).
- **Fix `/activities/inbox` too.** It uses the same DTO (`ActivityInbox.razor:13-27`). Its raw subject id is [T142]'s.
- Observed seed survey: 14 of 19 seed schemas carry exactly one `epa` field, all keyed `epa_id`. Add the pointer to
  each; the refresher republishes them. `msf_cpsa` credits nothing, so only the pointer can supply its EPA.

[T167] (the committee's evidence snapshot) depends on this stamp.

**Verification, added:**

- [x] `MyActivities` shows, for each completed activity, whether it credited, credited nothing, or was never
      evaluated. bUnit test, and in the browser.
- [x] A type whose schema pointer and credit `epa_field` disagree cannot be published. Test.
- [x] `EpaId` is stamped on create, on transition, and on the MSF release path. Application tests.
- [x] `/activities/inbox` shows each row's EPA and encounter date. bUnit test.

---

## As built — 2026-09-24 (with [T106] item 14)

- **`evidence_epa_field`**, a schema-root pointer to the form's `epa` field (Parse + Serialize, a round-trip fixture,
  a builder picker "EPA field"). It is named for what the field is, the EPA this activity is evidence for, which also
  covers types that credit nothing. It cannot be misread as the credit directive's `epa_field`.
- **Agreement, refused rather than defaulted** (`EvidenceEpa.EnsureCreditAgrees`, in `SaveDraft` and `PublishDraft`
  before anything is assigned). There are two shapes:
  - One EPA: a pointer, every `epa_field` equal to it, and no item-targeted directive.
  - None: no pointer, and no `epa_field`.
  Defaulting a missing `epa_field` to the pointer was rejected: credit, the tool gate, the rebuild and the picker would
  each have had to learn it. See CUSTOMIZATION.md § Schema format.
- **`Activity.EpaId`** is stamped by `EvidenceEpaResolver` at create, on every transition, and on the MSF release path,
  and resolves only to an existing EPA.
- **Migration `20260924123852_T137_ActivityEvidenceEpa`:**
  - adds an index;
  - adds the pointer to stored schemas where the version's own credit or its only `epa` field decides it, published
    versions included (an inert addition, recorded as a W-007 exception in CUSTOMIZATION.md § Versioning);
  - backfills `EpaId` from the data, parsing ids as `int.TryParse` does.
- **Lists:**
  - `ActivitySummaryDto` carries `EpaId`, `EpaCode`, `EpaTitle`, `ObservedOn`, `ObservedOnDeclared` and the last credit
    outcome (three-valued, T108).
  - `MyActivities` shows Type, EPA, Encounter date ("(filed; no encounter date)" when undated), State and Credited, and
    sorts by encounter date. The inbox adds EPA and Encounter date.
  - `CreditOutcome.Label` is the one wording, shared with `ActivityView`.
- **Evidence.**
  - Suites on master: Domain 398, Application 958, Infrastructure 658, Architecture 28, Web 384, Integration 28 (5 new
    Postgres migration tests). `has-pending-model-changes` is clean. The mutants across both rounds were all caught.
  - Browser on dev (the migration applied at startup; the refresher republished 0 of 19):
    - EpaId: 21 of 21 activities with an `epa_id` are stamped. The six MSF rows carry PAED-001, 010, 012, 002, 005 and
      007 and read apart on `/activities/mine`.
    - Order and dates: the list order matches `ObservedOn DESC, UpdatedOn DESC, Id DESC`, and undated rows 17, 21 and
      23 show "(filed; no encounter date)".
    - Credit column: "1 item" and "—" were seen. "None" could not be, because no dev transition has
      `CreditedItemCount = 0`; the bUnit tests cover it.
    - Inbox: it shows EPA and encounter date.
    - New activity: Mini-CEX 24, filed against PAED-007, was stamped `EpaId = 8` by the create INSERT.
- Noticed, not T137's: dev's MSF activities 1–5 carry encounter dates after today (scenario data; [T160] refuses that
  going forward). The trainee dashboard's recent-activities card could show the EPA code now; [T142] widens the same
  surfaces.

