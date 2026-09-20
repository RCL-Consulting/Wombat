---
id: T120
title: "Author the ten remaining v11.1 WBA tools, and separate the five an engineer can write from the five only the College can"
status: queued
priority: P1
created: 2026-09-19
---
# T120 — Author the ten remaining v11.1 WBA tools, and separate the five an engineer can write from the five only the College can

**Status:** open
**Surfaced:** 2026-09-19, planning the remaining EPA work after the Wave-0 evidence run ([T118]).
**Severity:** Medium. Nothing is broken and nothing is unreachable — every one of the 15 EPAs already has
at least one seeded tool (verified below). What is missing is fidelity: ten of the College's fourteen
instruments cannot be filed at all, so a registrar's 55 annual encounters are funnelled through four tools
the College never intended to carry the whole load, and the per-EPA tool allow-list the College actually
published is inert data in the repo.

## The definitive list

From `execution/tasks/done/T098-data/annexure-a.json`, whose `tools` field is **free text** — an allow-list per
EPA with the count in parentheses, and **no per-tool sub-quota anywhere in the source**. Parsed, the
fourteen tools and the EPAs naming each:

| Tool | EPAs naming it | # | Seeded? | Key to use |
|---|---|---|---|---|
| MSF | 1–15 (all) | 15 | no | *not an activity type — see below* |
| CBD | 1,2,3,4,5,8,9,11,12,13,14,15 | 12 | ✅ `cbd_cpsa` | — |
| Mini-CEX | 1,2,3,4,6,7,8,12,13 | 9 | ✅ `mini_cex_cpsa` | — |
| DOPS | 1,2,3,4,5,6,7,8 | 8 | ✅ `dops_cpsa` | — |
| Direct observation | 2,4,6,9,10,11,13,15 | 8 | ✅ `direct_observation_cpsa` | — |
| CCA | 1,2,3,4 | 4 | no | `cca_cpsa` |
| Reflective exercise | 1,3,8,14 | 4 | no | `reflective_exercise_cpsa` |
| RCA | 1,2,3 | 3 | no | `rca_cpsa` |
| Clinical audit | 1,2,3 | 3 | no | `clinical_audit_cpsa` |
| Chart-stimulated recall | 2,12 | 2 | no | `chart_stimulated_recall_cpsa` |
| Case note review | 6 | 1 | no | `case_note_review_cpsa` |
| Directly observed clinical examination | 7 | 1 | no | `observed_clinical_exam_cpsa` |
| Learner feedback | 15 | 1 | no | *MSF-shaped — see below* |
| Portfolio and logbook review | 15 | 1 | no | `portfolio_review_cpsa` |

Integrity check: the per-tool counts sum to 72, and so do Annexure A's own parenthesised per-EPA counts
(8+9+8+6+3+5+4+5+3+2+3+4+4+3+5). The parse is faithful.

### The keys are not free choices

`GetEpaTrajectoryForTraineeQuery.cs:77-90` matches an activity to a WBA source family by **exact key or a
`"<family>_"` prefix**, and it already contains `cca`, `rca`, `case_note_review`,
`chart_stimulated_recall` and `observed_clinical_exam` — added speculatively by T098 so "tools named by
v11.1 but not yet seeded chart the moment they are created". Name a seed anything else and it silently
never appears on a trajectory. The `_cpsa` suffix (never `_paed`, never a `cpsa_` prefix) is the rule
`PaediatricCatalogueSeeder.cs:78-90` already states and explains.

Note what that map's contents tell you: the five families it anticipates are exactly the five tools that
produce an entrustment rung from a single named assessor. The four it does not name — Clinical audit,
Reflective exercise, Learner feedback, Portfolio and logbook review — are not oversights. They are a
different kind of instrument. That split is the spine of this task.

### Nothing today enforces the per-EPA allow-list

`paediatric-epa-v11.1.json` carries `wbaTools` as a proper string array on every EPA, and `currency` too.
`EpaSeed` (`PaediatricCatalogueSeeder.cs:397-405`) declares neither property, so both are **deserialized
by nothing**. Adding these ten types widens what a trainee may pick; nothing checks that a CCA was filed
against an EPA whose allow-list contains CCA. See decision **D9**.

### None of the ten unlocks an EPA — state this before anyone prioritises on the wrong premise

Cross-referencing the allow-lists against the four seeded tools: every EPA from 1 to 15 already permits at
least one of Mini-CEX, DOPS, CBD or Direct observation. The thinnest are EPA 10 (`MSF, Direct observation`
— one seeded) and EPA 14 (`MSF, Reflective exercise, CBD` — one seeded). So the value of this task is
**tool-mix fidelity and the College's own allow-list**, not reachability. If the argument for doing it is
"a registrar cannot record their evidence", that argument is false. The true argument is that a portfolio
built entirely from four tools does not look like the one v11.1 describes, and that the CCC has no
case-analysis and no reflective evidence to weigh.

## What the repo knows about each of the ten, and what it does not

Descriptive prose exists in exactly one place: the `frequency` field of
`execution/tasks/done/T098-data/epa-detail.json`, which for some EPAs expands each tool into a sentence of usage
guidance. It is **not** a form specification — it says what the tool is for, never what fields it has.
`annexure-a.json` has no prose. `paediatric-epa-v11.1.json` has none either (its `wbaTools` array is bare
strings). The T098 task file adds nothing per tool.

There is also a hole the document itself points at. Every EPA's `assessment` field reads *"The standard set
applies to this EPA — see 'Standard assessment information sources' on page 8"*, and **page 8 was never
extracted**. `EPA version 11.1.docx` is in the repo root. If that page defines the instruments, it
collapses half the decisions below, and it costs one extraction to find out. See **D10** — do it first.

| Tool | What the repo knows | Verdict |
|---|---|---|
| **CCA** | Two *different* descriptions. EPAs 1 and 3: *"review clinical notes and discuss cases involving critically ill children, focusing on clinical reasoning, decision-making and management plans."* EPA 2: *"structured review of the trainee's own admission notes, daily progress notes and discharge summaries against the institutional standard."* The abbreviation is never expanded anywhere. | **Shape inferable from `cbd_cpsa`** (a rated conversation about a case). **The content must come from the College** — the document does not agree with itself about whether this is a discussion or a notes audit (**D2**). |
| **RCA** | EPA 3: *"select cases at random from the registrar's patient records to identify knowledge gaps and areas for improvement."* EPA 2: *"review of clinical notes and outcomes in acute admissions."* Listed on EPA 1 in Annexure A but **absent from EPA 1's prose** — the prose is incomplete against the table. | **Shape inferable from `cbd_cpsa`.** The prose says *random case analysis*; "RCA" in clinical governance reads as *root cause analysis*, and this string goes straight into a picker a registrar reads (**D4**). |
| **Chart-stimulated recall** | EPA 2: *"questioning based on the trainee's own records, to probe the reasoning behind documented ward decisions."* One clear sentence. | **Inferable.** It is `cbd_cpsa` with the case reference being a chart rather than a remembered case. The lowest-risk of the ten. |
| **Case note review** | **Nothing.** A bare listing in EPA 6's `tools` string and in its one-line WBA list. No prose in any file. | **Must come from the College** — or be resolved as a duplicate of EPA 2's CCA, which is literally described as a case-note review (**D3**). |
| **Directly observed clinical examination** | EPA 7: *"well-baby and well-child examinations."* | **Inferable.** It is `mini_cex_cpsa` with an examination-type field whose two options the source supplies. |
| **Reflective exercise** | EPAs 1 and 3: *"reflect on challenging cases or critical incidents, discussing experiences and learning points with a mentor or supervisor."* EPAs 8 and 14 list it without prose. | **Shape inferable from the existing `reflective_note` seed** (a STAR-framed self-authored note with a sign-off). What the College must decide is not the fields but whether it is rated and whether it counts (**D5**, **D6**). |
| **Clinical audit** | EPA 1: *"audit specific aspects of paediatric emergency care, such as infection control practices or adherence to resuscitation protocols."* EPA 2: *"of an aspect of ward care such as prescribing accuracy, early-warning-score compliance or discharge summary completeness."* EPA 3 repeats EPA 1. | **Fields largely inferable** (title, aim, standard, sample, findings, change, re-audit date — `qi_project`'s PDSA seed is the nearest existing shape). **The artefact is not**: an audit without its report attached is a claim (**D7**). |
| **Portfolio and logbook review** | EPA 15: *"teaching sessions, journal club presentations, feedback received, and reflective entries."* | **Blocked on what it reviews**, not on its fields. It reviews a body of work Wombat already holds. See Group 2. |
| **Learner feedback** | EPA 15: *"structured evaluation by students, junior trainees and team members taught by the registrar."* Plus EPA 15's own rule: *"feedback from at least two assessors across at least two teaching contexts, before entrustment is granted."* | **Not an activity type.** Respondents are plural and are not the named assessor. See Group 3. |
| **MSF** | Per-EPA usage prose on several EPAs (EPA 1: *"collect feedback from a range of colleagues, including nurses, other doctors and allied health professionals, as well as from patients' families"*). | **Not an activity type.** Already a dedicated aggregate. Cross-referenced in Group 3. |

## Grouping by engineering difficulty

### Group 1 — five plain seeds

`cca_cpsa`, `rca_cpsa`, `chart_stimulated_recall_cpsa`, `case_note_review_cpsa`,
`observed_clinical_exam_cpsa`.

All five are the same shape as the four that exist: one trainee requests, one named assessor rates on the
six-rung CPSA ladder and writes three feedback fields. Everything structural is inferable — `epa_id`,
`assessor_user_id`, `observed_on`, the `overall_level` scale field, the three longtexts, the five-state
workflow, the one-directive credit rule. Only the `request` section's context fields are tool-specific,
and for four of the five the source prose says what they are. Each is an afternoon once its decision is
answered.

The deltas from `src/Wombat.Infrastructure/Activities/Seeds/mini_cex_cpsa/schema.json`:

- `cca_cpsa` — replace `presenting_problem` and `complexity` with a case reference and a
  records-reviewed field; keep `setting`.
- `rca_cpsa` — a case reference plus how the case was selected. The point of the instrument is that the
  case was picked at random, so the form should record that it was.
- `chart_stimulated_recall_cpsa` — a chart or record reference and the decisions probed.
- `case_note_review_cpsa` — unknown until **D3** is answered; if it survives as a distinct tool it is
  `cca_cpsa` with a records-type field instead of a case reference.
- `observed_clinical_exam_cpsa` — `mini_cex_cpsa` with `presenting_problem` replaced by an
  `examination_type` choice: `well_baby`, `well_child`, plus whatever the College adds.

#### The exact checklist for one plain seed

1. Create `src/Wombat.Infrastructure/Activities/Seeds/<key>/` with `schema.json`, `workflow.json` and
   `credit.json`. Copy `mini_cex_cpsa` (rated observation) or `cbd_cpsa` (rated conversation) and edit the
   `request` section only.
2. **The key must be `<family>_cpsa`** where `<family>` is one of the five already present in
   `GetEpaTrajectoryForTraineeQuery.cs:77-90`. A different key is not an error; it is silence — the tool
   works, credits, and never appears on any trajectory.
3. **Register it in `ActivityTypeSeedCatalogue.Entries`**
   (`src/Wombat.Infrastructure/Persistence/ActivityTypeSeedCatalogue.cs:76-112`) with
   `ActivityScope.Speciality`, `ActivityTypeSeedSource.PaediatricCollege` and `DisplayFieldsRule.None`.
   Without that line the folder is dead weight: `PaediatricCatalogueSeeder.EnsureActivityTypesAsync:102`
   iterates the catalogue, not the disk, and `ActivityTypeSeedRefresher` (T103) never sees an unregistered
   folder. CLAUDE.md says this; it is easy to forget because the seed *file* is where the work felt like it
   happened.
4. Give it a **display name no other type shares**. [T118] finding 6: the picker already offers two types
   called "Mini-CEX (Paediatrics)" and two called "DOPS (Paediatrics)", and a trainee who picks the wrong
   one gets no credit. Do not add to that.
5. Rating field: `"key": "overall_level"`, `"type": "scale"`,
   `"scale_key": "CPSA Paediatric Entrustment Scale v11.1"` — the scale's **exact Name**, because it binds
   by string and a wrong one degrades silently to a number box — `"validation": {"min": 1, "max": 6}`, and
   **no `options` array**. Declaring options would cap the validator below the sixth rung and override the
   College's labels in the picker. `CpsaWbaSeedTests.RatingFieldDeclaresNoOptions` and
   `RatingFieldBindsToTheCollegeScaleByExactName` assert both.
6. Ownership is declared, never inferred: `"editable_by": "field:assessor_user_id"` on the `requested`
   **workflow state** and on the `assessment` and `feedback` **schema sections**; the `request` section
   keeps the default. `CpsaWbaSeedTests.AssessorOwnershipIsDeclaredOnTheRequestedStateAndTheAssessorSections`
   asserts the last part with teeth — an assessor-owned request section would let a `complete` patch
   rewrite `epa_id` and redirect which curriculum item is credited.
7. `completed` is the **only** terminal state. Credit fires on any transition into a terminal state, so
   marking `declined` or `cancelled` terminal would count refused and withdrawn requests toward the
   trainee's observation volume (`OnlyCompletionIsTerminal`).
8. Until [T105] lands, assessor fields carry **no** `required: true`; they are gated by `requires_fields`
   on `complete`. After T105 they carry honest `required` flags. Pick whichever is true at the time and
   match `CpsaWbaSeedTests.AssessorFieldsAreNotSchemaRequired` to it.
9. `credit.json` is one directive: `curriculum_item_match.epa_field = "epa_id"`, `amount: 1`,
   `minimum_level_field: "overall_level"`. That last property is what puts the comparison on the pinned
   scale (T109); omitting it silently makes the tool credit volume with no supervision gate at all.
10. **Use only field types that render.** `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor:37-104` handles `text`, `longtext`, `number`,
    `date`, `choice`, `multichoice`, `scale`, `user`, `epa`, `rating`, `likert`, `signature` and
    `procedure_ref`. `datetime`, `checkbox` and `markdown` fall through to *"Unsupported field type"*, and
    `file` renders a placeholder at `:98-102`. This is [T106] item 8, now verified.
11. **Add the key to `CpsaWbaSeedTests.SeedKeys`**
    (`tests/Wombat.Infrastructure.Tests/Activities/CpsaWbaSeedTests.cs:20-25`). It is a hard-coded list of
    four, not a discovered one, so a new seed is silently untested otherwise. `SeedRoundTripTests` does
    discover folders (`:289`) and picks the new one up for free.
12. If — and only if — you add a **new DSL property**, write its `Serialize` half as well as its `Parse`
    half and extend `SeedRoundTripTests`. `ActivityType.SaveDraft` stores `Serialize(Parse(json))`, so a
    parse-only property is dropped at publish with no error; and once the refresher compares
    canonical-to-canonical, both sides drop it identically and report "in sync" for ever.
13. Run `dotnet test tests/Wombat.Infrastructure.Tests/Wombat.Infrastructure.Tests.csproj` — **without**
    `--no-build`.
14. Boot twice. The first boot creates each type at version 1; the second must report no difference. A seed
    that is not a canonicalisation fixed point republishes on every start, for ever.

### Group 2 — platform-blocked

**`clinical_audit_cpsa` (EPAs 1, 2, 3) and `portfolio_review_cpsa` (EPA 15) both want an attachment, and
the platform has no file storage at all.** Verified: `ActivityForm.razor:98-102` renders the literal string
*"File uploads are represented in the schema, but storage wiring lands in a later pass."* — a placeholder,
not an input. `SchemaValidator.cs:78` treats a `file` value as a plain string. A grep across `src` for
`InputFile`, `IBrowserFile` or `IFormFile` returns **nothing**, and no folder under `src/Wombat.Domain/`
holds an attachment entity. `FieldType.File` is a reserved word, not a feature.

That is a bigger decision than it looks, because an attachment is personal data: it has to appear in
`AccessReportBuilder`'s subject access export, be handled by `ErasureExecutor`, survive the portfolio PDF
and be served under the CSP. It deserves its own task rather than riding in on a seed. See **D7**; the
interim is a `text` field holding a URL or a document reference, which is at least honest about being a
pointer.

`portfolio_review_cpsa` has a second and more interesting problem: it reviews *"teaching sessions, journal
club presentations, feedback received, and reflective entries"* — a body of work Wombat already holds, as
`teaching_session` and `journal_club` activities and as the T023 portfolio PDF. A form asking the reviewer
to re-type what the product can already assemble is the wrong shape. The right shape is a review that
names the trainee and a period and points the reviewer at the existing export. That is a design question,
not a form question, and it is why this one is last.

**`reflective_exercise_cpsa` (EPAs 1, 3, 8, 14) is *not* platform-blocked** — it is decision-blocked, and
the distinction matters for scheduling. The shape already exists: `reflective_note` is a STAR-framed
self-authored note whose `submit` transition is `actor: "subject"` and whose approval is
`role:SpecialityAdmin+scope:speciality`. The source says the reflection is discussed *"with a mentor or
supervisor"*, which argues for `field:assessor_user_id` instead — consistent with the rest of the CPSA set,
and automatically covered by [T102]'s refusal to let a trainee name themselves as their own assessor. What
is undecided is whether it carries a rung and whether it counts (**D5**, **D6**). Answer those and it is a
plain seed with an interesting history.

**`learner_feedback` cannot be expressed by the actor grammar at all.** The respondents are *"students,
junior trainees and team members taught by the registrar"* — plural, and not the named assessor. Three
things break at once: `field:assessor_user_id` resolves to exactly one user id (`ActorRuleMatcher.Matches`
→ `FieldUserActorRule`); `GetAssessorOptionsAsync` (`ActivityReferenceDataService.cs:252-256`) only ever
offers users **in role `Assessor` within the caller's institution**, so a medical student would not appear
even if they had an account; and students largely do not have accounts. This is MSF's shape — several
respondents, aggregation, anonymity — so it belongs there.

### Group 3 — not an activity type

**MSF (all 15 EPAs) and Learner feedback (EPA 15).** `src/Wombat.Domain/MultiSourceFeedback/` already
implements the hard parts: token-based respondent access without a login, per-category aggregation
thresholds (`MsfCampaign.MinimumResponses = 8`, `MinimumCategoryResponses = 3`), a coordinator review step
and a release gate. What it lacks is any connection to the rest of the model: **no `EpaId` anywhere in the
folder, and no credit path** — `CreditApplier.ApplyAsync` takes an `Activity`, and an `MsfCampaign` is not
one. That is its own task. **Cross-reference it rather than duplicating its design here**, and do not let
this task's "ten tools" framing imply MSF is a seed folder waiting to be written.

Learner feedback should ride on that work as an `MsfTemplate` with a new `Learner` value on
`MsfRespondentCategory` (`src/Wombat.Domain/MultiSourceFeedback/MsfRespondentCategory.cs`, today
`PeerDoctor` / `Consultant` / `Nurse` / `Ahp` / `Patient` / `Other`). One thing it will need that MSF does
not have: EPA 15 requires *"at least two assessors across at least two teaching contexts"*, which is a
per-**context** threshold, not a per-category count, and `MinimumCategoryResponses` cannot express it.
Flag it there (**D8**).

## Prerequisites

### [T105] should land first, but it is a preference, not a hard block

Today the four CPSA seeds encode a workaround: `overall_level`, `strengths`, `improvements` and `plan`
carry **no** `required: true`, because `src/Wombat.Infrastructure/Activities/ActivityService.cs:203-207` validates the **whole schema in
Submit mode on every transition** — so a required assessor field would also block `decline` and `cancel`. They are
gated by `requires_fields` on `complete` instead, and `CpsaWbaSeedTests.AssessorFieldsAreNotSchemaRequired`
asserts that arrangement as the intended one.

Write five more seeds now and the workaround is baked into **nine of fourteen** types. When T105 lands,
each of the nine needs its schema re-authored and that test inverted — a mechanical edit, but nine times,
and `ActivityTypeSeedRefresher` will then republish all nine, bumping every version and stranding any
in-flight activity on the old one ([T107]). Doing T105 first means the new seeds say what they mean from
the first line, and `requires_fields` goes back to meaning "additionally required for this step" rather
than "the only place requiredness is recorded".

The honest counterweight: if T105 slips, writing the five seeds under the current convention is not a trap.
It is the same convention the existing four already use, it is tested, and the later edit is mechanical.
**Do not block this task on T105 indefinitely.** Block it on the College (**D1**, **D10**), which is the
real critical path.

### [T106] item 8 — verified, and it constrains the schemas

Checked against `ActivityForm.razor`: `datetime`, `checkbox` and `markdown` render *"Unsupported field
type"*, and `file` renders a placeholder (`:98`). Item 8 can be closed with that answer. For this task it
is a hard constraint — checklist step 10.

### Things that are *not* prerequisites

- **[T110]** (`or_scale` resolves to nothing) does not touch these. The CPSA seeds bind by the scale's
  exact Name and already resolve.
- **The `observed_on` wiring task** ([T119]) **has since shipped** — corrected 2026-09-20. It was not a
  prerequisite and still is not, but the warning that stood here is now wrong: credit runs off
  `Activity.ObservedOn`, and `CreditApplier.ResolveObservationDate` with its `CreatedOn` fallback no
  longer exists. The five new seeds should capture `observed_on` and it **will** be honoured.
- **[T104]** is not a prerequisite, but the display-name rule in checklist step 4 exists because of it.

### One live consequence worth recording while you are here

`GetSamplingConcentrationWarnings.cs:92-99` maps activity keys to WBA source categories by **exact key**
and contains only `mini_cex`, `dops`, `cbd` and `acat`. None of the four `*_cpsa` keys is in it. So the
committee's sampling-concentration report **already counts nothing at all for a v11.1 trainee** — the
denominator is zero and the warnings are empty, silently. T098 trap 6 noted that two divergent taxonomies
exist; it did not say that one of them has been blind to the entire paediatric catalogue since the day it
was seeded. Adding five more `*_cpsa` keys makes that worse unless the map is fixed in the same change.
Its enum already carries unused `LongitudinalObservation` and `ProductEvaluation` members, which is
where MSF and audit/portfolio evidence belong.

## Recommended order

1. **Extract page 8 of `EPA version 11.1.docx`** (**D10**). One extraction, and it may answer D2, D3, D4
   and D5 outright. Do not commission ten form designs from busy clinicians before reading the page every
   EPA points at.
2. `chart_stimulated_recall_cpsa` and `observed_clinical_exam_cpsa` — both have explicit source prose and
   an unambiguous parent shape. Ship them together as the pattern-setting pair, and fix
   `SourceByActivityKey` in the same change.
3. `cca_cpsa` **and** `case_note_review_cpsa` together, because D2 and D3 are one decision taken twice.
4. `rca_cpsa`, once D4 settles the name.
5. `reflective_exercise_cpsa`, after D5 and D6.
6. `clinical_audit_cpsa`, after D7.
7. `portfolio_review_cpsa` last — it needs a design, not a form.
8. MSF and Learner feedback in the MSF task, not here.

### The minimum useful subset if the College is slow

`cca_cpsa` (EPAs 1, 2, 3, 4) and `rca_cpsa` (EPAs 1, 2, 3). Those two widen the permitted mix on the four
acute EPAs that carry 24 of the 55 annual encounters, and both are case-analysis instruments — a category
the portfolio currently has **none** of, since all four seeded tools are observation or conversation. Add
`chart_stimulated_recall_cpsa` (EPAs 2, 12) third; it is the cheapest of the ten and it is the only other
tool on EPA 12.

But say the true answer out loud: **the single highest-value missing tool is MSF**, named by all fifteen
EPAs, and it is not in this task. If only one thing gets done, it should be that one.

## Decisions for the maintainer and the College

**D1 — Is v11.1 final, or is the "DRAFT — FOR DISCUSSION" cover current?** Carried from T098 open question
1 and still unanswered. It gates asking the College for ten form designs, which is a real ask of real
people. *Recommendation: settle this before D10.*

**D2 — What is CCA, and which of its two descriptions is right?** The abbreviation is never expanded in the
source; EPAs 1 and 3 describe a case discussion while EPA 2 describes a structured audit of the trainee's
own notes. *Recommendation: treat it as case-based **c**linical **a**ssessment — a rated discussion of a
case with the notes in front of both parties — which reconciles the two descriptions. Needs College
confirmation, because the field set differs between readings.*

**D3 — Is "Case note review" (EPA 6) a distinct instrument from EPA 2's CCA?** There is no prose for it
anywhere. *Recommendation: ask whether it is a duplicate. If it is, seed one type and name it on both
EPAs' allow-lists — one fewer form for the College to design, and one fewer near-identical entry in a
picker that already has a name-collision problem.*

**D4 — What does RCA stand for here?** The prose describes random case selection; the abbreviation reads as
root cause analysis to every clinician who meets it in a picker. *Recommendation: seed the key as
`rca_cpsa` — the trajectory map requires that — but set the **display name** to "Random Case Analysis
(Paediatrics)". The key is never shown to a user.*

**D5 — Which of the ten produce an entrustment rung, and which are unrated evidence?** This decides whether
`credit.json` carries `minimum_level_field`. *Recommendation: the five in Group 1 are rated; Reflective
exercise, Clinical audit and Portfolio review are unrated; MSF and Learner feedback are handled by MSF.
This is what the trajectory map already assumes, so agreeing with it costs nothing.*

**D6 — Does an unrated tool count toward the "six per annum"?** If yes, a reflective exercise increments
`CountsSoFar` with no supervision evidence behind it and can help fill a quota. If no, it carries no
`counts_for` directive at all and is documentation rather than assessment. *Recommendation: yes for volume,
never for the minimum — which is exactly what an omitted `minimum_level_field` already does (T109's
`NoGate` basis). The College should still confirm, because it changes what "55 encounters" means.*

**D7 — File attachments: build storage, or accept a URL field for now?** Clinical audit and Portfolio
review both want a document. Real storage means a new entity, a store on the Linode host, size and type
limits, inclusion in `AccessReportBuilder`'s subject access export, handling in `ErasureExecutor`, and a
CSP review. *Recommendation: a `text` field holding a link now, and attachments as their own task later.
An attachment is personal data and should not arrive as a side effect of a seed folder.*

**D8 — Learner feedback: an MSF template, or a separate token-based form?** *Recommendation: MSF, with a
new `Learner` value on `MsfRespondentCategory`. It reuses the anonymity, thresholds and token issuance
that already exist and would otherwise be rebuilt badly. Flag in the MSF task that EPA 15's "two assessors
across two teaching contexts" is a per-context threshold `MinimumCategoryResponses` cannot express.*

**D9 — Should the per-EPA tool allow-list be enforced, or stay advisory?** `wbaTools` is in the seed file
and read by nothing. *Recommendation: deserialize it, store it, and **display** it on the EPA so a trainee
can see which tools are permitted — but enforce nothing yet. Enforcement is really a tool-mix rule and
belongs with phase 3's per-period counters, not with a picker filter.*

**D10 — Extract page 8, "Standard assessment information sources".** Every EPA's `assessment` field points
at it, it was never extracted, and `EPA version 11.1.docx` is in the repo root. *Recommendation: do this
first. It is the cheapest action in the whole task and it may remove four of the nine decisions above.*

## Verification

- All five Group-1 seeds exist on disk, are registered in `ActivityTypeSeedCatalogue`, and are created by
  `PaediatricCatalogueSeeder` on a fresh database.
- A second boot reports no seed difference — the canonical form is a fixed point and no version churns.
- `CpsaWbaSeedTests` runs over nine keys, not four, and passes every assertion for each.
- A trainee on curriculum 3 files one of the new tools against a permitted EPA, an assessor rates it at
  3a, and it credits: `CreditedItemCount = 1`, `CreditScaleMismatchCount = 0`, `CountsSoFar` increments.
  This is [T118]'s evidence run repeated with a new tool, and it is the only proof that matters.
- The observation appears on the trainee's EPA trajectory with the right source label. That is the check
  that catches a mis-named key, and nothing else catches it.
- The type picker shows each new tool under a name no other type shares.

## Related

The remaining half of [T098] gap 5 (*"8 of the 14 WBA tools have no seeded activity type"* — the count was
8 because it was written before `direct_observation_cpsa` was seeded and it did not separate MSF and
Learner feedback from the rest; the true figure is ten missing, of which two are MSF-shaped). Prefers
[T105] first. Verifies and closes [T106] item 8. Constrained by [T118] findings 6 and 7 on picker
ambiguity, which [T104] resolves. MSF and Learner feedback belong to the MSF credit-path task. The unread
`wbaTools` and `currency` properties are the same omission, and `currency` is already claimed by T098
phase 3.
