# The EPA programme

The one file to run the remaining EPA work from. It does not replace the task files — each row below
points at one — it says what is outstanding, what is blocked on a decision only a human can take, and in
what order the pieces have to land.

Read `execution/STATE.md` first for where the last session stopped. Read this for where the
programme is going.

---

## 1. Where the EPA work stands

**The v11.1 catalogue credited for the first time on 2026-09-19.** Before that date it was seeded but had
never been used: no trainee on curriculum 3, no adoption row, nothing ever credited. Every severity
judgement about it was a guess.

The run, through the UI on dev: institution 2 adopted `Paediatric EPA Curriculum 11.1`; Ndlovu was moved
from curriculum 2 to curriculum 3; she filed a `mini_cex_cpsa` against PAED-001 with `observed_on`
**2026-03-10**; Naidoo completed it at **3a**. Result: `CreditedItemCount = 1`,
`CreditScaleMismatchCount = 0`, one `CurriculumItemProgress` row on item 17 with `CountsSoFar = 1` and
`MinimumLevelReachedCount = 1`. Full detail in [`execution/tasks/queued/T118-v11-1-evidence-run-findings.md`].

**What that proved, on real data rather than by reading code:** T109's scale pinning holds (both sides
resolve to the six-rung CPSA ladder, no mismatch); T073's per-stage minimum works (the item's
`MinimumLevelOrder` is 6, the year-1 minimum of 3 was applied, so 3a counted); T101's scope stamping
follows a curriculum move; T070's field ownership disables the assessor's fields for the trainee and
enables them for the assessor; T108's EPA narrowing offers exactly the 15 v11.1 EPAs. The credit engine is
sound. **What is missing from here on is fidelity and time, not correctness.**

**What the same run disproved.** The progress page reads **"1 / 24"** for an EPA requiring six per annum,
because `PaediatricCatalogueSeeder.cs:356-361` multiplies the annual quota by four programme years — a
registrar cannot tell what is expected of them this year. The encounter was March and the trajectory plots
September, because `CreditApplier.ResolveObservationDate` (`CreditApplier.cs:335`, called at `:56`) dates
everything from `Activity.CreatedOn`. The rung picker reads `1. 1 · 2. 2 · 3. 3a · 4. 3b · 5. 4 · 6. 5`.
The type picker offers two indistinguishable "Mini-CEX (Paediatrics)".

**Current pinning, verified on dev 2026-09-19:** curriculum 1 (`IM Core`) 1/1 items pinned to a scale;
curriculum 2 (`FCPaed(SA) Part 1`) **0/15**; curriculum 3 (`Paediatric EPA` 11.1) **15/15**. Seeders pin;
T109's migration deliberately backfills nothing. Curriculum 2 is unpinned only because no seeder authors
it.

**Nothing is live.** No real users, only regenerable scenario data. Backward compatibility is not a
constraint anywhere in this programme, and the word should not appear in an argument for a design.

**Production is unverified.** Nothing this month touched `wombat.rcl.co.za`. Its adoption rows, its
speciality-scope rows ([T099] is done on dev only) and its pinning state are unknown. The four legacy
`*_paed` activity types are **operator data that exists in no seeder** — nothing in `src` replays them, so
every data decision below has to be re-run by hand on production.

---

## 2. The complete inventory

Size: **S** = a session or less · **M** = one focused session · **L** = several sessions.
Verdict: **READY** = an implementer can start today · **NEEDS A DECISION** = one of §3 must land first.

| Task | What it is | Verdict | Size | Depends on |
|---|---|---|---|---|
| [T119] | Wire `observed_on`: a declared schema pointer, a stamped `Activity.ObservedOn` column, every reader on the column | **DONE 2026-09-19** | M | shipped. `CreditApplier` credits off `Activity.ObservedOn`; `ResolveObservationDate` and its `CreatedOn` fallback are gone. Unblocks [T130] |
| [T123] d1 | Trajectory axis and labels resolved from the pinned scale server-side | **DONE 2026-09-19** | S | step 2 and full D30 deferred to [T126]; re-check charts with T110 |
| [T123] d3 | Narrow the activity-type picker by the subject's pinned ladder (T109's unshipped option 2) | **DONE 2026-09-19** | S | SQL check passed; ⚠ must be sequenced with [T110] |
| [T123] d2 | Two "Mini-CEX (Paediatrics)", two "DOPS (Paediatrics)" in the picker | NEEDS A DECISION | S | **D31**. Data edit, no code |
| [T111] | `/activities/new?type=mini_cex` is silently ignored | **DONE 2026-09-19** | S | — |
| [T099] | Speciality-3 scope rows on **production**; nothing provisions them on a fresh database | **READY** | S | production access |
| [T100] | Entrustment rungs render as `"{Order}. {Label}"` — 6 sites; 8 more print a bare ordinal; 1 PDF site prints a raw `DataJson` integer | **DONE 2026-09-19** | M | premise rewritten (it was wrong twice); admin editor split out as [T125] |
| [T105] | Every transition validates the whole schema in Submit mode, so a half-filled draft cannot be cancelled | NEEDS A DECISION | M | **D22** |
| [T102] fixes 2–3 | Server-side validation of `user`-typed field values (fix 1 shipped with T070) | NEEDS A DECISION | M | **D23** |
| [T110] | `scale_key: "or_scale"` resolves to nothing for four generic seeds; two five-rung ladders are the same ladder duplicated | NEEDS A DECISION | S code, operator data | **D25** |
| [T107] | Activities pinned to a superseded schema version are uncompletable and the UI still offers the button | NEEDS A DECISION | S | **D33** |
| [T121] | **MSF is required by 11 or 15 EPAs — the source says both (D37) — and can credit none.** Synthetic activity on release, `MsfCampaignEpa` join, `msf_cpsa` seed | NEEDS A DECISION | L | **D8–D11**. Design is complete and blocked by nothing else |
| [T120] group 1 | Five plain seeds — `cca_cpsa`, `rca_cpsa`, `chart_stimulated_recall_cpsa`, `case_note_review_cpsa`, `observed_clinical_exam_cpsa` — plus fixing `GetSamplingConcentrationWarnings.cs:92-99`. **[T124] argues this is really three**: page 8 defines CCA as documentation review + reasoning discussion, which is what "case note review" and "chart-stimulated recall" describe | NEEDS A DECISION | M | **D1, D4, D6** (D2/D3/D5 closed). Prefers T105 and T102 fix 2 |
| [T120] reflective | `reflective_exercise_cpsa` | NEEDS A DECISION | S | **D6, D7** |
| [T120] group 2 | `clinical_audit_cpsa`, `portfolio_review_cpsa` — both want an attachment and `FieldType.File` is a reserved word, not a feature | NEEDS A DECISION | L | **D34**. Attachments are their own task |
| [T122] | The EPA→tool allow-list is parsed by nothing: every CPSA tool can credit every CPSA EPA | NEEDS A DECISION | M | **D12, D20, D21**. Composes with T123 d3 |
| [T130] (was [T098] phase 3) | **The annual quota.** Semester buckets on `CurriculumItemProgress`, `AcademicPeriod` + `QuotaWindow`, per-period targets, one shared read model behind the progress page and all five progress readers | **DONE 2026-09-23** | L | shipped. D17–D19 closed, D39–D42 decided (§ 3E). Four College questions ride on it (§ 3F); each is a read-model or one-constant change, because storage is per semester |
| [T098] phase 4 | Governance: neonatal CCC routing for EPAs 4–5, semester cadence, an EPA agenda on reviews | NEEDS A DECISION | L | phase 3 |
| [T104] | Retire the legacy FCPaed world — 4 activity types, 15 EPAs, curriculum 2, 5 trainee profiles | NEEDS A DECISION | M, hand-run | **D24**. If D24 says re-pin, also curriculum 2 pinned + a documented ordinal remap |
| Rebuild fixes | `RebuildCurriculumProgressCommand` has **no caller** (only `ActivityReadBoundaryTests.cs:60`), is **not atomic** (`:30-32` deletes and saves before the replay saves at `:71`), and does not stamp `CreditedItemCount` ([T106] item 12) | **READY** | S | rides T119, which is the change that makes a rebuild necessary |
| [T106] item 14 | A zero-credit completion is visible one activity at a time; "55 logged, nothing counted" is a statement about a year | **READY** | S | — |
| [T125] | An admin sets a curriculum minimum by typing a bare integer against an invisible ladder — an unguided write, not a mislabelled read | **READY** | S | split out of [T100] |
| [T126] | Nothing can say which ladder a given activity was rated on: no declared rated field, no navigation to the pinned version | **READY** | M | blocks [T123] d1 step 2 and full D30 |
| [T127] | A failed Submit leaves an orphan draft, and the retry makes another | **READY** | S | compounds with [T105] |
| MSF defects | `MsfInvitationExpiryReminderJob.cs:57` mails the token **hash** as the token, so every expiry reminder is a dead link; `ListMsfCampaignsForCoordinator` returns every campaign in every institution; no MSF command takes a `ClaimsPrincipal` | **READY** | S / M | the reminder link is on [T121]'s critical path — a campaign that never reaches 8 responses can never be released and never credits |
| [T118] | Holding file | — | — | closes as its findings land: 1–2 → T100, 3 → phase 3, 4 → T119, 5–7 → T123, 8 → already closed as not-a-defect |

[T106] items 8 (three field types render nothing) is **closed** by T120's verification of
`ActivityForm.razor:37-104`. Item 10 (`WindowMonths` read by nothing but `AdmitTrainee.cs:126`) is
decision **D19**.

---

## 3. THE DECISION LIST

> **Scope: product decisions only, `D1`–`D42`.** Decisions about *how the project is run* —
> tooling, workspace layout, process — live in `execution/DECISIONS.md` under a `W-nnn`
> prefix, deliberately distinct so a bare `D30` can only ever mean this register.

This is the section to work through. Nothing below can be inferred from the source, the code or the
repository — each is a judgement, and every one of them is currently being made by default rather than
deliberately.

### 3A. Closed

**C1 — What is the period anchor for "six per annum"? — CLOSED 2026-09-19.**
**The fixed national academic year, subdivided into two semesters.** Annexure B: *"about 5 a month across
an eleven-month academic year … 25 of the 55 fall in each semester"*. Chosen over trainee-anchored 365-day
blocks, which would have been free (`TraineeProfile.GetStage`, `TraineeProfile.cs:66-75`, already computes
exactly that and `CreditApplier` already uses it), because a shared boundary is what makes a departmental
"how are we doing this semester" view computable at all. **Consequence: `GetStage` and the period resolver
now disagree by construction — see D17.**

**D2 — Does page 8 define the instruments? — CLOSED 2026-09-19. No.** Page 8 holds *two* sections and the
EPAs cite the second: "Workplace-based assessment (WBA) tools" defines **nine** tools in one sentence each,
and "Standard assessment information sources" lists **four information sources** (clinical documentation ·
colleagues · patients, guardians and family · the candidate) that ground every summative decision. No field
sets, no per-tool scale, no structure. **The form designs still have to be commissioned.** Full extraction
in [T124]; data in `T098-data/page-8-wba-tools.json`.

**D3 — What is CCA? — CLOSED 2026-09-19. "Clinical Case Analysis"** — *"review of clinical documentation
and discussion of the reasoning and management plan recorded."* The reconciling reading recommended here
was right; the expansion guessed here was wrong. Seed `cca_cpsa`, display "Clinical Case Analysis
(Paediatrics)". No split needed — EPA 1/3's "case discussion" and EPA 2's "notes audit" are both instances
of the one definition.

**D5 — What does RCA stand for? — CLOSED 2026-09-19. "Random Case Analysis"** — *"review of cases selected
at random from the trainee's records to identify knowledge gaps."* Recommendation confirmed verbatim.

### 3A-ii. Closed by the College's reply, 2026-09-20

> The reply is recorded verbatim in `knowledge/college-rfi-v11-1.md`, answers written inline,
> in two passes: eight came back first, and the remaining six after D6, D12, D13, D8, D7 and D15
> were put back. **All fourteen are now answered.**
>
> Two things were NOT settled by the reply and must not be assumed from it: the **scope of the
> "clinical observed interaction" merge** (see D12 — merging Mini-CEX with Direct observation makes
> EPA 10, a leadership EPA, creditable by a Mini-CEX), and **whether June falls in the first
> semester or the second** (see D13). Both are one line from the College.

**D1 — Is v11.1 final? — CLOSED 2026-09-20. Yes, treat it as final and build from it.** The College's
words: *"Treat 11.1 as final and build from that"* — option (a) of three. The "DRAFT — FOR DISCUSSION"
cover is not current. This was the gate on asking for anything else, and it is open. **Consequence:
[T120]'s critical path is no longer D1; commissioning form designs against v11.1 is now a reasonable
ask.** If a v11.2 lands, the catalogue versions rather than mutates (T091).

**D4 — Is "Case note review" distinct from CCA? — CLOSED 2026-09-20. No, it is an alias.** Option (b)
confirmed. EPA 6's *"Case note review"* and EPAs 1–4's *"CCA"* are the same instrument: page 8's
Clinical Case Analysis, *"review of clinical documentation and discussion of the reasoning and
management plan recorded"*. **Consequence: `case_note_review_cpsa` is not a seed.** `cca_cpsa` is named
on EPAs 1, 2, 3, 4 **and 6**, which drops the instrument count from fourteen to thirteen and gives
[T122]'s allow-list one fewer row to enforce. The alias must be recorded where a reader of Annexure A
will look for it, or the mapping stops matching the published table line by line.

**D9 — MSF per period or per EPA? — CLOSED 2026-09-20. Per period, covering many EPAs.** Proposed
default confirmed. Per EPA would have been 15 × 8 = **120 returned questionnaires per registrar per
year**; per period is about 16. **Consequence: `MsfCampaignEpa` is the join that expresses it, and
"MSF completed for EPA 7" means "a campaign covering EPA 7 was released this period", not "a campaign
about EPA 7".** Say that wherever the phrase is printed.

**D10 — Who states the level an MSF asserts? — CLOSED 2026-09-20. The releasing reviewer.** Proposed
default confirmed: one ordinal recorded by a named clinician alongside the summary they already write,
never a mean of respondent scores — *"a mean of 3.4 is not a rung on a ladder reading 1, 2, 3a, 3b, 4,
5"*. Option (a), asking respondents directly, was the only one needing a questionnaire redesign, and it
was not chosen. **Consequence: the field is optional and authored at release, so `MsfQuestion.ScaleId`
stays nullable and no aggregation changes.**

> **A second consequence, found building [T121] on 2026-09-21 and worth stating because it was not
> foreseen here.** Keeping the optional ordinal means `msf_cpsa` carries a `scale` field, and
> `SeedRoundTripTests.Schema_DeclaresARatedFieldExactlyWhenItCarriesAScale` makes that a biconditional:
> a schema with a `scale` field MUST declare `rated_level_field`. So MSF is now in the **rated** set
> alongside the nine of D6 — nine seeded tools, not eight. It is nonetheless invisible on both surfaces
> that read that set, because neither can find an `assessor_user_id` on it: the committee sampling
> report skips it in the numerator and the trajectory chart does not plot it. That is the right answer —
> an MSF asserts a level but names no observing assessor — reached by an accidental mechanism, which is
> why it is pinned by name in `SeedScaleKeyTests.ExactlyNineSeededToolsAreRated` and recorded on [T135].
> **The only thing the College may want to rule on is whether an MSF ordinal should appear on a
> trainee's entrustment trajectory at all.** Today it does not, deliberately.

**D11 — Must more than one respondent group respond? — CLOSED 2026-09-20. Yes, at least two
categories must survive suppression.** Proposed default confirmed. A campaign answered entirely by
eight peer doctors currently passes the release threshold and would credit, showing one category and
the rest suppressed — which is not multi-source feedback. **Consequence: a predicate on the release
gate, which changes when campaigns can be released.**

**D14 — A registrar starting mid-year? — CLOSED 2026-09-20. Exempt the partial period.** Proposed
default confirmed: they start counting at the next boundary, stated plainly on the progress page.
Pro-rata was rejected because it invents a fraction the College never published, and carry-forward
because it makes the second period's target unreadable. **Consequence: a rule in [T130]'s period
resolver, not a target calculation.**

**D16 — Are the 78 descriptors individually assessable? — CLOSED 2026-09-20. No, narrative scope.**
Option (a) confirmed, which is the status quo: `PaediatricCatalogueSeeder` joins them into
`Epa.RequiredKnowledgeSkills` as narrative. **Consequence: zero work, and the largest unplanned item in
the programme is now formally off the table.** Entrustment is judged on the EPA as a whole.

**D37 — Is MSF on eleven EPAs or fifteen? — CLOSED 2026-09-20. Fifteen.** Annexure A's side confirmed
against Annexure B's prose, which said eleven. **Consequence: [T121] may state 15/15 as fact rather
than as one of two readings, and D9's arithmetic stands at 120 questionnaires per registrar per year
under the rejected per-EPA design.** Annexure B's sentence is wrong and should be treated as such
wherever it is quoted.


**D6 — Which instruments produce an entrustment level? — CLOSED 2026-09-20. Nine rated, three
unrated.** The first reply read *"default all rated"*; put back with the observation that this was not
the proposed default, the College corrected it to the nine/three split as proposed.
**Rated** (a named assessor states a level): CBD, Mini-CEX, DOPS, Direct observation, CCA, RCA,
Chart-stimulated recall, Directly observed clinical examination — and Case note review, which D4 makes
an alias of CCA rather than a tenth. **Unrated evidence**: Reflective exercise, Clinical audit,
Portfolio and logbook review. **Consequence: nothing changes.** [T120]'s Group split stands, [T126]'s
`rated_level_field` policy holds, and the trajectory query's rule that only assessor-rated tools belong
on an entrustment chart is not contradicted — which "all rated" would have done, since a reflective
exercise is trainee-authored and rating it would have been a trainee stating their own entrustment
level ([T102]'s defect class, by design rather than oversight).

**D7 — Does an unrated instrument count toward the annual frequency? — CLOSED 2026-09-20. No.**
Option (b), **not** the proposed default (a, volume only). An unrated instrument is documentation, not
assessment, and consumes none of an EPA's published encounters. **Consequence: the three unrated
instruments carry `"counts_for": []`.** That is also what keeps them clear of the hazard this question
carried: a directive with neither `minimum_level_field` nor `minimum_level_fixed` returns `NotGated()`,
which is `MinimumMet: true` — the trainee recorded as having met the supervision minimum on every EPA
it touched. `counts_for: []` is not that; an empty target list is not an ungated directive.

**D8 — Does MSF consume the 55 encounters? — CLOSED 2026-09-20. No.** Option (c): MSF is required
evidence tracked in its own right. The 55 keeps meaning the sum of Annexure A's per-EPA quotas.
**Consequence, and it is why this one had to be right first time: `counts_for` is permanent per pinned
version.** `Activity.SchemaVersion` is assigned exactly once, there is no re-pin path anywhere
([T107]), and a rebuild replays each activity against its **pinned** version — so whatever `msf_cpsa`
v1 ships with is permanent for every activity created under v1, in both directions. "Ship `[]` now and
switch when it settles" was never available. `msf_cpsa` ships `counts_for: []` from v1.

**D12 — Does EPA 7 exclude general Direct observation? — CLOSED 2026-09-20. No, it does not.**
**Consequence: EPA 7's allow-list gains Direct observation**, and `observed_clinical_exam_cpsa` is not
written — the College's first reply said a Mini-CEX and a clinical examination are the same thing, and
EPA 7 already permits Mini-CEX, so a second seed would be a duplicate in the picker.

> **One piece is still open and must not be assumed.** The first reply also proposed merging Mini-CEX
> into a new instrument, *"clinical observed interaction ... a mini cex, handover, communication etc."*
> Re-derived from `T098-data/annexure-a.json`: **Mini-CEX** is named by EPAs 1, 2, 3, 4, 6, 7, 8, 12,
> 13; **Directly observed clinical examination** by EPA 7 only; **Direct observation** by EPAs 2, 4, 6,
> 9, 10, 11, 13, 15. Merging Mini-CEX with Directly observed clinical examination changes **no EPA's
> permitted set** — EPA 7 already names both. Merging Mini-CEX with **Direct observation** changes
> eight: EPAs 9, 11 and 15 gain a focused-encounter tool, EPAs 1, 3, 8 and 12 gain ward-round
> observation, and **EPA 10 — "Leading and operating within a clinical team", published allow-list
> exactly "MSF, Direct observation (2)" — becomes creditable by a Mini-CEX.** That is the defect class
> [T122] exists to prevent. Note too that the reply's own example list named *handover*, which is page
> 8's definition of Direct observation, not of Mini-CEX. **Ask before merging.**

**D13 — The academic year and the semester boundary — CLOSED 2026-09-20. January to November, with
the boundary in June.** Eleven months, matching Annexure B. National, as proposed — a per-institution
boundary would give two registrars in the same national programme different targets in the same month.
**Consequence: [T130] is unblocked.** One line is still wanted before it buckets anything: **whether
June itself falls in the first semester or the second.** Recorded here as Jan–Jun / Jul–Nov, which is
the reading of "boundary in June" this register has taken; a June encounter is the only thing that
moves if it is wrong.

**D15 — A deadline for filing after the encounter? — CLOSED 2026-09-20. A soft warning beyond
fourteen days, and no hard refusal.** Fourteen, not the ninety proposed. **Consequence: this is new
work, not a change.** Verified 2026-09-20 that no observation-date staleness warning exists today —
the only 90-day constants in the repository are the portfolio-export and scheduled-job-run retention
jobs. Fourteen days is tight enough that it will fire routinely, so the "no hard refusal" half is
load-bearing: a registrar blocked by a date validator types today's date instead, which destroys the
encounter date that [T119] exists to protect.
---

### 3B. For the College / CPSA content owner

These go in one message. They are the critical path for [T120] and [T121], and they are asks of busy
clinicians, so batch them.

**D1 — Is v11.1 final, or is the "DRAFT — FOR DISCUSSION" cover current?**
The cover of `EPA version 11.1.docx` says the content is subject to revision; the operator described the
EPAs as finalised. T091 made the catalogue College-owned and institution-adopted, so publishing it is a
national act.
*Options:* treat as final and build · hold the ten tools until confirmed · build against it and version the
catalogue when v11.2 lands.
**Recommendation: settle this before asking for anything else.** It costs one email and it gates a real ask
of real people.

**D2 — Does page 8, "Standard assessment information sources", define the instruments?**
Every EPA's `assessment` field reads *"The standard set applies to this EPA — see 'Standard assessment
information sources' on page 8"*, and **page 8 was never extracted.** `EPA version 11.1.docx` is in the
repo root.
*Options:* extract it now (free, one action) · commission ten form designs from the College without reading
it · infer from the per-EPA `frequency` prose, which says what a tool is *for* and never what fields it has.
**Recommendation: extract it first.** It may answer D3, D4, D5 and D6 outright. Do not commission form
designs before reading the page every EPA points at.

**D3 — What is CCA, and which of its two descriptions is right?**
The abbreviation is never expanded. EPAs 1 and 3 describe a case discussion; EPA 2 describes a structured
audit of the trainee's own admission notes against the institutional standard. The field set differs
between readings.
*Options:* case-based **c**linical **a**ssessment (a rated discussion with the notes in front of both
parties, reconciling the two) · a notes audit · two separate tools.
**Recommendation: the reconciling reading, subject to College confirmation.**

**D4 — Is "Case note review" (EPA 6) a distinct instrument from EPA 2's CCA?**
There is no prose for it anywhere in the source — a bare listing in EPA 6's `tools` cell.
*Options:* distinct tool with its own seed · alias of CCA, named on both EPAs' allow-lists.
**Recommendation: ask, and hope for alias.** One fewer form for the College to design, and one fewer
near-identical entry in a picker that already has a name-collision problem.

**D5 — What does RCA stand for here?**
The prose describes selecting cases at random; the acronym reads as *root cause analysis* to every clinician
who meets it in a picker.
*Options:* display "Random Case Analysis (Paediatrics)" · display "RCA" verbatim as published.
**Recommendation: key `rca_cpsa` (the trajectory family map requires it), display name spelled out.** The
key is never shown to a user.

**D6 — Which of the fourteen tools produce an entrustment rung, and which are unrated evidence?**
This decides whether each `credit.json` carries `minimum_level_field`, which is what binds the comparison
to the pinned scale (T109).
*Options:* the five Group-1 tools rated, reflective exercise / clinical audit / portfolio review unrated ·
everything rated · everything unrated.
**Recommendation: the first.** It is already what `GetEpaTrajectoryForTraineeQuery.cs:77-90` assumes — the
five families it anticipates are exactly the five that yield a rung from one named assessor — so agreeing
with it costs nothing.

**D7 — Does an unrated tool count toward the per-annum frequency?**
A reflective exercise would increment `CountsSoFar` with no supervision evidence behind it.
*Options:* yes for volume, never for the minimum · not at all (`"counts_for": []`, documentation rather than
assessment) · yes for both.
**Recommendation: volume only** — which is exactly what an omitted `minimum_level_field` already does. The
College should still confirm, because it changes what "55 encounters" means.
⚠ **Do not implement "volume only" by omitting the gate entirely.** A directive with neither
`minimum_level_field` nor `minimum_level_fixed` returns `NotGated()` (`CreditApplier.cs:295-299`), which is
`MinimumMet: true`, and would record the trainee as having met the supervision minimum on every EPA it
touched. That is T109's defect in a new costume. Use an **optional** level field, which falls to
`ValueMissing` (`CreditApplier.cs:321`) and touches no anomaly counter.

**D8 — Does MSF count toward the 55 observed encounters per year?**
Annexure A's frequencies sum to **exactly 55**, so the 55 *is* the sum of the per-EPA quotas. If an MSF
credits 1 against each covered EPA, two semester campaigns supply **30 of the 55 from two questionnaires**,
and for the five EPAs whose quota is "one per annum" a single MSF discharges the entire year.
*Options:* (1) yes, uncapped — simplest, inflates the counter from day one · (2) yes, capped at one per
period per EPA — needs phase 3's period key and a per-tool counter, not expressible today · (3) no — MSF is
required evidence tracked in its own right, a one-line seed edit (`"counts_for": []`).
**Recommendation: 3 now, 2 once phase 3 exists.** That keeps the 55 meaning what the College says it means,
and makes [T121]'s value the *evidence link*, not the count.

**D9 — Is MSF run once per period covering many EPAs, or once per EPA?**
The arithmetic is one-sided. `MinimumResponses` defaults to 8 (`MsfCampaign.cs:12-13`) and release is
refused below it. Per EPA: 15 × 8 = **120 returned questionnaires per registrar per year**; a 20-registrar
department puts ~80 questionnaires a year on each consultant and ward nurse, response rates collapse, and
nothing ever reaches `ReadyForRelease`. Per semester covering many EPAs: 16 per registrar per year, ~16 per
respondent.
**Recommendation: per period.** But it changes what "MSF on EPA 7" means on a certificate, so the College
should say it. `MsfCampaignEpa` expresses either — this decision changes guidance and workload, not schema.

**D10 — Who states the entrustment level an MSF asserts, if anyone?**
MSF produces no usable rating today: `MsfQuestion.ScaleId` is nullable and `null` for every template the
product can create, scale answers are not range-checked (`MsfCampaignRules.cs:81`), and the only scalars are
per-(category, question) arithmetic means as `double` (`MsfAggregationService.cs:50-53`). A mean of 3.4 is
not a rung on a ladder reading `1, 2, 3a, 3b, 4, 5`.
*Options:* the **respondents** (needs a new scale question and a median, never a mean; but respondents
include nurses, AHPs and patients, who are not positioned to judge entrustment) · the **releasing
reviewer** (one ordinal beside the narrative they already write, a named clinician standing behind it) ·
**nobody** (MSF credits volume and asserts nothing).
**Recommendation: the reviewer, with "nobody" an acceptable default.** The field is optional either way, so
the two cost the same code. Only the respondent option needs College sign-off.

**D11 — Does a campaign with suppressed categories still credit?**
`ReadyForRelease` counts total responses only (`MsfAggregationService.cs:80`); the per-category minimum only
suppresses a category from the report (`:26-29`). A campaign answered by eight peer doctors releases, shows
one category and five suppressed, and would credit.
*Options:* add "at least N categories survive suppression" to the release gate · leave it.
**Recommendation: add it, if the College's view is that MSF means *multi*-source.** One predicate in
`ReleaseMsfCampaignCommandHandler`, but it changes when campaigns can be released, so it is the
maintainer's call on the College's steer.

**D12 — Seed the fourteen tool names verbatim, or collapse them with aliases?**
Annexure A contains pairs that may denote one instrument: "Direct observation" (**8** EPAs — 2, 4, 6,
9, 10, 11, 13, 15; re-derived from `T098-data/annexure-a.json` 2026-09-20, correcting "7" here) vs "Directly
observed clinical examination" (PAED-007 only); "CCA" vs "Case note review".
**Consequence of verbatim, stated plainly:** the seeded `direct_observation_cpsa` would be permitted on
PAED-006 and **refused on PAED-007**. That may be exactly what the College means, or an artefact of one
document written by several hands.
**Recommendation: seed all fourteen verbatim and ask the College to merge explicitly.** A merge Wombat
invents is invisible and unauditable; a merge the College makes is recorded in the catalogue file, and the
mapping's whole value is that it can be checked against the published annexure line by line.

**D13 — When does the academic year start and end, and where does the semester boundary fall?**
Annexure B says eleven months; it does not say which. This is not in the repo and cannot be derived.
*Options:* a constant · configurable per College · configurable per institution.
**Recommendation: per College.** The whole point of C1 is a *shared* boundary, and a per-institution
boundary would give two registrars in the same national programme different quotas.

**D14 — What happens to a registrar who starts mid-year?**
They do not get eleven months before the first boundary.
*Options:* pro-rata the quota · exempt the partial period · carry forward into the next.
**Recommendation: exempt the partial period and say so on the progress page.** Pro-rata invents a fraction
the College never published; carry-forward makes the second period's target unreadable. But this is a
College rule, not a product one.

**D15 — Is there a deadline for submitting an assessment after the encounter?**
Annexure A and B are silent as read. Nothing bounds backdating today: `SchemaValidator.ValidateDateField`
(`:166-172`) checks only that the string parses.
*Options:* a hard refusal beyond N days · a soft warning · nothing.
**Recommendation: ship [T119]'s soft 90-day warning, ask, and harden only if the College says so.** A hard
limit invented here produces a registrar who types today's date to get past the validator — which is
precisely the defect [T119] exists to repair, re-created by its own guard. [T119] specified two rules
that need no policy input (not in the future, not before `ProgrammeStartDate`). **Neither is enforced in code**
(verified 2026-09-23 during [T130]: `SchemaValidator.ValidateDateField` checks only that the string parses, and
`ObservationDateResolver` bounds nothing). [T130]'s calendar is total over every date for exactly that reason.

**D16 — Do the 78 descriptors need to be individually assessable, or are they narrative scope?**
Today they are carried as narrative: `PaediatricCatalogueSeeder.cs:262-268` joins them into
`Epa.RequiredKnowledgeSkills` with a comment saying exactly that.
*Options:* narrative (status quo, zero work) · a first-class descriptor entity with its own credit path
(schema change, large).
**Recommendation: narrative, and confirm it.** If the answer is "assessable", it is the largest single
unplanned item in the programme and needs its own task before anything else is built on top.

**D37 — Is MSF specified on eleven EPAs or on fifteen? The document says both.** *(new 2026-09-19, [T124])*
Annexure A lists `MSF` in the `tools` cell of **15 of 15** EPAs. Annexure B's prose says *"Multi-source
feedback is specified in **eleven** of the fifteen EPAs."* Same document, same version.
**This does not change any recommendation** — MSF is the most-required tool in the catalogue either way,
and [T121]'s defect stands — but it changes the arithmetic quoted to the College in D8 and D9: D9's "15 × 8
= 120 questionnaires per registrar per year" becomes 88, and D8's "30 of the 55" becomes 22.
**Recommendation: one line in the same message as D1.** Cheap to ask, and [T121] opens with Annexure A's
side stated as fact.

**D38 — Does a committee decision have to record what it was grounded in?** *(new 2026-09-19, [T124])*
Page 4 of the source is unambiguous: *"The summative entrustment decision for an EPA is taken by the
Clinical Competency Committee, drawing on the standard assessment information sources set out on page 8 —
**never by a single assessor and never from a single form**."* Wombat today lets a committee record an
entrustment decision with **no stated evidence basis at all**.
*Options:* a phase-4 requirement (the decision names the evidence it drew on) · guidance only · nothing.
**Recommendation: a phase-4 requirement, written into phase 4's task file** — which does not exist yet
(§5 item 1). It is the College's own words about what a decision *is*, not a product preference. Note this
is the maintainer's call on scope, not a content question, so it does not need to go to the College.

### 3C. For the maintainer — these gate a wave

**D17 — `GetStage` and the period resolver disagree by construction. Which one moves? — CLOSED 2026-09-23 ([T130]), as recommended: two named concepts.**
`TraineeProfile.GetStage` stays the 365-day "training year" and still selects the per-stage minimum; `AcademicPeriod`
is the new, separately named calendar and selects the bucket. UI copy says "training year N" for one and
"Semester S, YYYY" / "YYYY academic year" for the other, and the progress page says when a training year changed
inside a period (the only place the two visibly interact).
Stage is a 365-day block from `ProgrammeStartDate` (`TraineeProfile.cs:66-75`) and drives T073's per-stage
minimum. The period is a fixed calendar window (C1).
*Options:* re-anchor `GetStage` to the academic year, so "training year" and "period" are one concept ·
keep them as two named concepts with the difference documented · make the period a subdivision of the stage.
**Recommendation: two concepts, explicitly named and documented — `Stage` for the Y1–Y4 curve, `Period` for
the quota.** They answer different questions: which target applies to this trainee, versus which bucket
this encounter falls in. But decide it *out loud*: silently having two notions of "year" is how the lossy
`RequiredCount` happened in the first place.

**D18 — Does `RequiredCount` become per-period, or gain a sibling? — CLOSED 2026-09-23 ([T130]), as recommended: redefined per period, ×4 deleted.**
It is the target per `CurriculumItem.QuotaPeriod` window. The T130 migration rewrote the fifteen seeded rows that still
held the old lifetime multiple (and only those).
Today `PaediatricCatalogueSeeder.cs:356-361` multiplies the annual quota by four programme years, which is
why the page reads "1 / 24".
*Options:* redefine `RequiredCount` as per-period and re-seed (nothing is live, so this is free) · add
`RequiredCountPerPeriod` beside it and leave the lifetime total.
**Recommendation: redefine it, and delete the multiplication.** A lifetime total nobody asked for is what
made the number unreadable. If a lifetime figure is wanted later it is a multiplication in a read model,
not a stored column.

**D19 — Does phase 3 use `CurriculumItem.WindowMonths`, or leave it? — CLOSED 2026-09-23 ([T130]), as recommended: left alone.**
Documented on the property and relabelled "Completion window (months)" in the admin editor, with help text saying it
is not the target period and credit does not check it. Found on the way: the source's "expiry period if not
practised" is six months for EPAs 1, 2, 4, 5, yet all fifteen are seeded at 12 — filed, not fixed.
It exists, is validated (`ManageCurriculumItems.cs:41,57`), is admin-editable, is carried through clone —
and is read by nothing in the credit path. Its sole consumer is `AdmitTrainee.cs:126`. An implementer will
find a period-shaped column already there and assume currency is enforced.
*Options:* use it as the period length · leave it and name the new concept differently · delete it.
**Recommendation: leave it, and say so in the phase-3 task.** It is per-item currency, a different question
from the quota period, and quietly repurposing it would give one column two meanings.

**D20 — Where is the EPA→tool allow-list enforced?**
*Options:* (a) filter the EPA picker only — cheapest, and unenforced for anything arriving through
`Wombat.Api` or an in-flight draft · (b) reject at credit time — refuses at the one moment nobody can act,
and drops into T108's zero-credit banner whose copy points the reader at their curriculum, which is not
what was wrong · (c) credit but warn — a fourth comparison basis, a new counter, a new banner · (d) reject
at submit, with the picker agreeing.
**Recommendation: (d), with `CreditApplier` deliberately not re-litigating.** An allow-list can be edited
after an encounter was filed; refusing credit retroactively deletes evidence a trainee legitimately
collected under the rule in force at the time. One predicate, two callers — the [T108] shape.

**D21 — What may a tool with no recognised `WbaToolKey` credit?**
Null means "not a recognised WBA instrument", which is true of every builder-made type.
*Options:* unrestricted (fall through permissively) · refused where the EPA has an allow-list.
**Recommendation: permissive.** [T108] and [T109] both landed on the same rule: a restriction that fires
where the answer is unknown produces an empty picker or a silent refusal, and both are worse failures than
the one being fixed. ⚠ [T122] and [T123] d3 narrow the same two pickers from different directions —
**both must keep the permissive fallback or between them they will empty one.**

**D22 — How does a transition declare its validation scope ([T105])?**
Today `ActivityService.cs:203-207` validates the whole schema in Submit mode on every transition, so a
half-filled draft cannot be cancelled, and the four CPSA seeds encode the workaround: assessor fields carry
no `required: true` and are gated by `requires_fields` instead.
*Options:* infer from the target state (a state with no outgoing transitions validates in Draft mode) · an
explicit `validation: "none" | "draft" | "submit"` property on the transition · leave it and keep the
workaround.
**Recommendation: the explicit property.** Inference is the kind of convention this repository has been
bitten by four times. It is a workflow-DSL addition, so it needs the [T070] treatment: parse **and**
serialise, a builder affordance, and a `SeedRoundTripTests` entry — a property with no `Serialize` half is
dropped at publish with no error.
**Why it gates a wave:** write five more seeds first and the workaround is baked into **nine of fourteen**
types, each then needing its schema re-authored, its test inverted, and a republish that strands every
in-flight activity ([T107]).

**D23 — Does [T102] fix 2 land before the ten tools?**
Fix 1 shipped with T070 — a trainee can no longer name *themself* as their own assessor. Nothing yet
validates that a submitted `user`-typed value names someone who holds the required role or is inside the
caller's scope; `SchemaValidator.cs:77` routes `FieldType.User` to plain string validation.
*Options:* fix 2 first · ship the tools and fix 2 after · leave it.
**Recommendation: fix 2 first.** Every one of the ten new tools carries a `user` field, and the fix
generalises to any admin-built tool, which is the platform premise.

**D24 — [T104]: what happens to the five trainees on curriculum 2?**
*Options:* re-pin them to curriculum 3 — needs curriculum 2 pinned to its scale first and a documented
ordinal remap, because a legacy "4 = Independent" silently becomes v11.1's "4 = 3b" · leave them to finish
on curriculum 2 and apply the national catalogue to new intakes only.
**Recommendation: leave them.** A registrar mid-programme should not have their assessment history
reinterpreted, and the alternative is a hand-run live migration over 15 `EntrustmentDecisions` and 4
progress rows with no code to replay it on production. Choosing this also removes T104's only hard blocker
and lets the legacy world be retired by deactivation rather than by migration.

**D25 — [T110]: are "O-R Scale" and "Paed General Entrustment Scale" the same ladder?**
`DataSeeder.cs:123-127` seeds O-R as Observe only / Direct supervision / Indirect supervision / Independent
/ Supervises others; the browser-made scale in `scenario-paediatrics.md:160-166` is the same five rungs.
Two ids, one ten-Cate ladder — and the second is **operator data**, so the repo cannot confirm it.
*Options:* merge (re-point every reference, delete one) before pinning anything to either · leave both and
bind the four generic seeds to whichever the curriculum uses · leave it all alone.
**Recommendation: merge, then fix the binding by renaming the four seeds' `scale_key` to the exact name.**
A stable slug is the better long-term answer, but `CreateEntrustmentScaleCommandHandler:34-47` never sets
one, so a `NOT NULL UNIQUE` slug column would throw a raw index violation on the second admin-created scale
— handle the create path first or take the rename.
⚠ Fixing T110 makes every legacy chart gain a five-rung labelled axis at that moment. **T110 and [T123] d1
change the same pictures**; whichever lands second re-checks the other's charts in a browser.

### 3E. Decided in [T130], 2026-09-23

Recorded here because each would cost real work to reverse. The first is a correction of this register's own
instruction; the other three are readings of College answers that were silent on the point.

**D39 — The quota window comes from Annexure B's per-semester column, NOT from the `currency` string.**
Wave 4 below said to seed the period from `currency`. That string is the source's "expiry period if not practised"
(Annexure A's "Currency / status"), which Annexure B relabels as the entrustment-decision cadence: "each semester"
for exactly EPAs 1, 2, 4, 5, 10, 12. Annexure B separately publishes a per-semester observation figure for ten EPAs
(1–5, 10, 12 at 3; 6 at 2; 7 and 15 at 1) and none for the five at one per annum, and its own totals (25 per
semester, 55 per year) only reconcile with that column. Seeding from `currency` would have made EPAs 3, 6 and 7
annual. The catalogue now carries an explicit `observationsPerSemester` key; `currency` is deliberately
undeserialized, left for [T131] with a named entry in a guard test. *Rejected:* `currency` (breaks Annexure B's
arithmetic). **Contested, and with the College (§ 3F):** EPAs 3, 6 and 7's own pages say "performed annually", so
Annexure B's split may be planning guidance rather than a hard per-semester target.

**D40 — The calendar: semester 1 is January–June, semester 2 July–December; June is semester 1; December folds
into semester 2.** The resolver must be total — it runs inside credit, and nothing bounds an encounter date — so a
December encounter lands in semester 2 of its year and the page shows the College's nominal end (30 November) as
the period's end, with a December notice. The boundary lives in one month-and-day constant on `AcademicPeriod`,
so a mid-June answer is expressible; changing it is one line plus a rebuild. *Rejected:* December as next year's
semester 1 (equally total, but it would hand the new year a head start the College never described).

**D41 — Progress is stored per semester, whatever the item's window; D14 is applied when progress is read.**
The row key is (item, trainee, academic year, semester), computed from the encounter date and nothing else, so an
administrator changing an item's window or a trainee's start date needs no re-bucketing. A yearly figure is the sum
of two rows. The College's D14 exemption waives the *target*; credit still lands, still shows, and the transition
is still stamped with what it credited (a suppressed credit would stamp 0 and raise T108's banner, which blames the
curriculum). *Rejected:* rows at each item's own grain (stranded by an edit), and a per-activity ledger (cleaner,
but it replaces the table under every reader for a benefit the rebuild already gives).

**D42 — What "part-way through" means for D14 (provisional).** D14 was asked about "a registrar who starts
mid-year" and says nothing about how late is late. A literal reading exempts a registrar who starts on the first
working day of January — 1 January is a public holiday — from a whole year's annual targets. So: a **semester**
target applies if the programme started within the semester's first calendar month (an invented tolerance, named
as such); an **academic-year** target is waived only for a start on or after 1 July, which is the College's own
"mid-year" and invents nothing. Both rules live in `QuotaWindow.LatestOnTimeStart`; a change needs no rebuild.

### 3F. The next message to the College — one line each

None blocks anything: storage is per semester, so every answer below is a read-model change, one constant, or a
rebuild. They are listed so they go in one message.

1. **June** (D13): is June itself in semester 1 or semester 2 — and is the boundary a date in mid-June?
2. **December** (D40): which period does an encounter observed in December count towards?
3. **Per-semester figures** (D39): are Annexure B's per-semester figures a target *each* semester, or a planning
   split of the annual frequency? EPAs 3, 6 and 7's own pages say "performed annually".
4. **Late starters** (D42): is a registrar who starts in the first days of a period (e.g. the first working day of
   January) exempt for it? And is a registrar who finishes part-way through a period held to its target?

### 3D. Decisions inside one task

The recommendation stands unless overruled. The options and their costs are written out in the task file
named in the last column — these are here so the list is complete, not so they are re-argued.

| # | Question | Recommendation | Written up in |
|---|---|---|---|
| D26 | Do the five undated generic WBA seeds (`mini_cex`, `dops`, `cbd`, `acat`, `teaching_session`) gain a required `observed_on`? | Yes. An assessment without an encounter date is not evidence of anything | [T119] D1 |
| D27 | Does the encounter date drive the portfolio PDF, the committee review window and the sampling warnings, or only credit and the trajectory? | All of them. Two dates in one product with no rule for which is which is the ambiguity T119 exists to remove | [T119] D2 |
| D28 | Is a `CreatedOn`-sourced date visibly marked to the reader? | Yes, as a follow-up behind T100 — it reads better once the neighbouring label defects are fixed | [T119] D4 |
| D29 | Rung labels on a 40px axis margin: label or ordinal? | Label when ≤4 characters, ordinal otherwise; full label in the tooltip and the screen-reader table **unconditionally**, so the accessible copy is never poorer than the picture | [T123] D1a |
| D30 | Points on a different ladder from the axis? | Keep them, draw them hollow, exclude from the polyline, label them with their own scale's rung. Dropping them erases recorded evidence the trainee did not create | [T123] D1b |
| D31 | The duplicate-name rename: which four get renamed? | The **legacy** four, through `/admin/activity-types`. Four text edits, no code, and it marks the world that is going away. The refresher never touches names, so renaming the CPSA four would need code *and* a hand edit on each database | [T123] D2 |
| D32 | Should the default rung rendering be the label alone? | Yes. The College's own document says "3a", never "level 4". Keep the ordinal only in the admin scale editor, in its own column | [T100] |
| D33 | [T107]: stranded activities — suppress the impossible action, allow a re-pin, or migrate the handful? | Suppress now (small, stops the product lying), then decide re-pin vs migrate against a count of how many exist on production | [T107] |
| D34 | File attachments: build storage, or accept a URL field for now? | A `text` field holding a link now; attachments as their own task. An attachment is personal data — it has to appear in `AccessReportBuilder`'s subject-access export, be handled by `ErasureExecutor`, survive the portfolio PDF and pass the CSP. It should not arrive as a side effect of a seed folder | [T120] D7 |
| D35 | Learner feedback: an MSF template, or a separate token-based form? | An MSF template with a new `Learner` value on `MsfRespondentCategory`. It reuses anonymity, thresholds and token issuance that would otherwise be rebuilt badly. Flag that EPA 15's "two assessors across two teaching contexts" is a per-*context* threshold `MinimumCategoryResponses` cannot express | [T120] D8 |
| D36 | Should MSF plot on the trajectory chart? | Not under [T121]'s design. `TryParseObservation` requires an `assessor_user_id` and MSF has no honest value for it; naming the release reviewer "the assessor" would be a lie on a clinician-facing chart. Revisit with the "Evidence type" rename | [T121] adjacent 5 |

---

## 4. The waves

Each wave is a set that can run in parallel. The order between waves is a real dependency, not a
preference, except where it says otherwise.

### Wave 0 — the evidence run — **DONE (2026-09-19)**

One real assessment end to end on curriculum 3. It proved the credit engine and produced eight findings,
three of them new. Everything below is planned against evidence rather than against a reading of the code,
which was the point.

### Wave 1 — ask, extract, and fix what needs nobody

Four independent threads, all startable today.

1. **Send the College D1, D4, D6–D16 and D37.** One message. It is the critical path for Waves 3 and 4 and
   everything else runs while it is outstanding. D3 and D5 no longer need asking — page 8 answered them.
2. ~~**Extract page 8 (D2).**~~ **DONE 2026-09-19 — [T124].** It closed D2 (the answer is *no*: page 8
   defines nine tools in one sentence each and four *information sources*, and specifies no field sets),
   closed D3 (CCA = **Clinical Case Analysis**) and D5 (RCA = **Random Case Analysis**), and evidenced D4
   and D12: the document **defines nine tools and Annexure A uses fourteen**, with a clean partition —
   every tool on ≥3 EPAs is defined, every tool on ≤2 is not. It also extracted **Annexure B for the first
   time** (`T098-data/annexure-b.json`), which hands phase 3 a published per-semester column and phase 4 a
   decision cadence, and it found a **source contradiction about MSF** (D37).
3. ~~**[T119] — `observed_on`.**~~ **DONE.** `Activity.ObservedOn` is a real column and credit runs off
   it; `ResolveObservationDate` is gone. `RebuildCurriculumProgressCommand` was rewritten with it and
   still has **no production caller** — confirm it is transactional before giving it a button. The
   original instruction follows, kept because its warning still applies. Ship the
   stamp → boot (migration, refresher, restamper) → fix and run the rebuild → verify on Ndlovu's
   trajectory, whose point must move from 2026-09-19 to 2026-03-10. Fix `RebuildCurriculumProgressCommand`
   in the same session: it has no caller, it is not atomic, and it does not stamp `CreditedItemCount`. **Do
   not give it a button before it is transactional** — a failure between `:30-32` and `:71` leaves every
   trainee in the system with zero progress.
4. ~~**The clinician-facing reading of every number.**~~ **DONE 2026-09-19**, browser-verified on dev.
   [T100] tiers 1–3 (premise rewritten first — it was wrong twice), [T123] d1 (axis from the scale),
   [T123] d3 (picker narrowing — the SQL check passed: the four `_paed` types declare `scale_key` **"2"**,
   a raw id, and curriculum 3 pins scale 3, so all four drop off a v11.1 trainee's menu, which is more
   than the task predicted) and [T111]. **[T099] on production is the only part outstanding.**
   Three defects in the specified fixes were caught before they shipped — an HTML-injection hole in D29's
   axis label, an integer-stepper assumption about contiguous ordinals, and `ChartPoint.Label` not being a
   rung label at all. Split out: [T125], [T126], [T127].

**Unblocks:** phase 3 (which cannot bucket without a date), and a registrar being able to read their own
progress page.

### Wave 2 — make the platform ready for ten more seeds

**This wave exists for one reason: [T105] must precede the ten tools.** The four CPSA seeds encode a
workaround forced by Submit-mode-on-every-transition. Write five more seeds first and it is baked into nine
of fourteen types, each then needing a re-authored schema, an inverted test and a republish that strands
in-flight activities.

Runs in parallel: [T105] (D22) · [T102] fix 2 (D23) · [T110] + the ladder merge (D25) · [T107]'s suppression
(D33) · [T106] item 14.

Honest counterweight: if T105 slips, writing the Group-1 seeds under the current convention is not a trap —
it is the convention the existing four already use, it is tested, and the later edit is mechanical. **Do not
block Wave 3 on T105 indefinitely.** Block it on the College, which is the real critical path.

### Wave 3 — fidelity: the instruments

Three independent tracks. Start each as its decisions land.

- **[T121] — MSF.** The single highest-value item in the programme: the only tool Annexure A names for
  **every** EPA, already a complete working aggregate, and it cannot move a trainee's progress by one
  count. Blocked by nothing except D8–D11. *Opus.* Fix `MsfInvitationExpiryReminderJob.cs:57` first — it
  mails the token hash as the token, so a campaign cannot reliably reach the 8 responses release requires,
  and without release nothing credits.
- **[T120] group 1 — five seeds.** After D1–D6. Fix `GetSamplingConcentrationWarnings.cs:92-99` in the same
  change: it matches keys exactly against `mini_cex`/`dops`/`cbd`/`acat`, so the committee's sampling report
  has been silently blind to the entire paediatric catalogue since the day it was seeded, and five more
  `*_cpsa` keys make it worse.
- **[T122] — the EPA→tool allow-list.** After D20–D21. Composes with [T123] d3; both keep the permissive
  fallback.

**State the priority honestly:** none of the ten tools unlocks an EPA. Every one of the 15 already permits
at least one seeded tool. The value here is tool-mix fidelity and the College's own allow-list — not
reachability. If only one thing in this wave gets done, it is MSF.

**Unblocks:** a portfolio that looks like the one v11.1 describes, and a CCC with case-analysis and
reflective evidence to weigh.

### Wave 4 — the quota ([T098] phase 3) — **DONE (2026-09-23), as [T130]**

Shipped as its own task file. Progress is stored per semester and read against a per-window target through one
shared read model; the College's D14 is applied when reading; the progress page, the trainee dashboard and the
three staff dashboards all say "n of m" for a named period. The instruction that stood here — seed the period from
the `currency` string — was wrong, and is corrected by D39 (§ 3E). The migration empties the progress table on
every existing database, and `CurriculumProgressBootstrapper` rebuilds it at the next startup; the manual rebuild
is `/admin/curriculum-progress`.

### Wave 5 — governance ([T098] phase 4)

Per-EPA panel routing so EPAs 4–5 go to the neonatal CCC; semester cadence; an EPA agenda on reviews so
"6 decided each semester + 9 annually" can be scheduled and chased. Depends on Wave 4's period concept.
Also has no task file.

### Wave 6 — retire the legacy world

[T104] plus [T123] d2's rename, after D24. If D24 says "leave the five trainees on curriculum 2" — the
recommendation — this becomes deactivation plus a code change (`GetCurriculumProgressForTrainee.cs:67-80`
never filters `Epa.IsActive`, so flipping the flag hides nothing today) rather than a hazardous re-pin.
Either way it is a hand-run migration on production with no code to replay it, so it needs a runbook.

[T123] d3, landing in Wave 1, already removes the duplicate tools from a v11.1 trainee's menu — *subject to
the SQL check*. If the legacy types' `scale_key` is blank, the predicate falls through permissively by
design and Wave 6 is the only remedy. Check before assuming Wave 1 closed it.

---

## 5. What is still not planned, even now

Honest list. Each of these is known, none has a task file, and several are larger than things that do.

1. **[T098] phase 4 has no task file of its own beyond [T131].** Phase 3 shipped as [T130] on 2026-09-23. The biggest item in the programme (the annual quota)
   and the whole of governance exist as prose inside T098 and as decisions in §3. Writing the phase-3 task
   is the first action of Wave 4, not an optional tidy-up.
2. **File attachments.** D34 parks it behind a URL field. `FieldType.File` is a reserved word, not a
   feature: `ActivityForm.razor:98-102` renders a literal placeholder, `SchemaValidator.cs:78` treats the
   value as a plain string, and a grep across `src` for `InputFile`, `IBrowserFile` or `IFormFile` returns
   nothing. Clinical audit and portfolio review both want one. It needs its own task and it touches the
   data-rights export, erasure, the PDF and the CSP.
3. **Portfolio and logbook review needs a design, not a form.** It reviews *"teaching sessions, journal
   club presentations, feedback received, and reflective entries"* — a body of work Wombat already holds. A
   form asking the reviewer to re-type what the product can already assemble is the wrong shape. Nobody has
   designed the right one.
4. **Learner feedback** is cross-referenced to MSF (D35) and designed nowhere. It also needs a per-*context*
   threshold that `MinimumCategoryResponses` cannot express.
5. **A tool-mix rule.** D8 option 2 ("at most one of EPA 1's six may be an MSF") needs phase 3's per-period
   counters and has no task. Note that Annexure A supplies no per-tool sub-quota, so any cap is an
   invention and must be a decision, not an inference.
6. **A `SystemManaged` flag on `ActivityType`.** [T121] works around its absence with an actor rule; a
   trainee can still create a stray `msf_cpsa` draft they can never complete. `ActivityPermissionRule`
   exists as a table and a DbSet and is read by nothing.
7. **The MSF feature's own scope defects.** `ListMsfCampaignsForCoordinatorQuery` returns every campaign in
   every institution with no principal and no scope; no MSF command or query takes a `ClaimsPrincipal` at
   all — the folder predates T056. Same family as [T112] and [T117], recorded in [T121] as adjacent, filed
   nowhere.
8. **The two tool taxonomies are still two.** [T120] fixes `SourceByActivityKey`'s blindness to the
   paediatric catalogue and [T123] renames the trajectory's "Source" column to "Evidence type", but
   `SourceByActivityKey` and `SourceByActivityFamily` remain two hard-coded lists with different value
   types. [T122]'s `WbaToolKey` is the thing that could retire both; nothing says it will.
9. **The 78 descriptors** (D16). Carried as narrative text in `Epa.RequiredKnowledgeSkills`. If the College
   says they must be individually assessable, that is a new entity, a new credit path and the largest
   unplanned item here.
10. **Production.** Its adoption rows, speciality-scope rows and pinning state are unverified; [T099] is
    done on dev only; the legacy `*_paed` rows exist in no seeder and nothing replays them; [T104] has no
    runbook. The whole programme has been planned against dev.
11. **Grouped display of the synthetic MSF activities.** A campaign covering 8 EPAs puts 8 near-identical
    rows in a trainee's activity list. [T121] states the cost and does not pay it.
12. **Nothing plans how the 21 CCC decisions per registrar per year get scheduled or chased.** That is
    phase 4, which is item 1 on this list.

---

## Task files referenced

[T098] `execution/tasks/done/T098-epa-v11-adoption.md` · [T099] `execution/tasks/queued/T099-paediatric-catalogue-unreachable.md` ·
[T100] `…T100-entrustment-rung-label-display.md` · [T102] `…T102-assessor-self-assignment-escalation.md` ·
[T104] `…T104-retire-legacy-paediatric-data.md` · [T105] `…T105-transition-validation-scope.md` ·
[T106] `…T106-activity-platform-backlog.md` · [T107] `…T107-stranded-activities-pinned-to-old-versions.md` ·
[T108] `…T108-scope-without-adoption-silently-uncredited.md` · [T109] `…T109-cross-scale-credit-comparison.md` ·
[T110] `…T110-or-scale-key-mismatch.md` · [T111] `…T111-new-activity-type-query-parameter-ignored.md` ·
[T118] `…T118-v11-1-evidence-run-findings.md` · [T119] `…T119-wire-observed-on-as-the-encounter-date.md` ·
[T120] `…T120-remaining-v11-1-wba-tools.md` · [T121] `…T121-msf-cannot-credit-an-epa.md` ·
[T122] `…T122-enforce-epa-tool-mapping.md` · [T123] `…T123-evidence-run-ui-defects.md` ·
[T124] `…T124-page-8-extraction-findings.md` ·
[T125] `…T125-curriculum-item-minima-are-unguided-integer-writes.md` ·
[T126] `…T126-an-activity-does-not-know-which-ladder-it-was-rated-on.md` ·
[T127] `…T127-a-failed-submit-leaves-an-orphan-draft-behind.md`

Source data extracted from `EPA version 11.1.docx`: `execution/tasks/done/T098-data/annexure-a.json` ·
`execution/tasks/done/T098-data/annexure-b.json` (new, [T124]) · `execution/tasks/done/T098-data/page-8-wba-tools.json` (new, [T124]) ·
`execution/tasks/done/T098-data/epa-detail.json`.
