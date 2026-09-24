# The EPA programme

The one file to run the remaining EPA work from. It does not replace the task files — each row below
points at one — it says what is outstanding, what is blocked on a decision only a human can take, and in
what order the pieces have to land.

Read `execution/STATE.md` first for where the last session stopped. Read this for where the
programme is going.

**Re-baselined 2026-09-24** against the task register and a read-only survey of the EPA stream at `431e69e`. § 2 is
now the live queue, and the rows closed since 2026-09-19 are kept in § 2B. The operator has deferred the production
deploy ([T157]) until the stream is complete: *"Not deploying yet, we need to get the EPA stream completed."* W-007
applies throughout: compatibility with existing data is never a constraint.

---

## 1. Where the EPA work stands

**On 2026-09-24 (observed at `431e69e`):** the quota ([T130]), the tool allow-lists ([T122]), the nominee gate
([T102]), the transition validation scope ([T105]), the declared rated field ([T126]) and MSF evidence ([T121]) have
shipped. Nine of the twelve instruments in the College vocabulary have seeds; the three without are clinical audit and
portfolio review ([T154]) and learner feedback ([T164]). What remains is in § 2A: filing-path and evidence-integrity
fixes that are ready now, the committee's side of the programme, and the instruments that wait on a decision.

**The v11.1 catalogue credited for the first time on 2026-09-19.** Before that date it was seeded but had
never been used: no trainee on curriculum 3, no adoption row, nothing ever credited. Every severity
judgement about it was a guess.

The run, through the UI on dev: institution 2 adopted `Paediatric EPA Curriculum 11.1`; Ndlovu was moved
from curriculum 2 to curriculum 3; she filed a `mini_cex_cpsa` against PAED-001 with `observed_on`
**2026-03-10**; Naidoo completed it at **3a**. Result: `CreditedItemCount = 1`,
`CreditScaleMismatchCount = 0`, one `CurriculumItemProgress` row on item 17 with `CountsSoFar = 1` and
`MinimumLevelReachedCount = 1`. Full detail in [`execution/tasks/done/T118-v11-1-evidence-run-findings.md`]
(closed 2026-09-24).

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

**All four are closed** ([T118]'s closing table, 2026-09-24): the "1 / 24" by [T130]'s per-period targets, the
encounter date by [T119], the rung labels by [T100], and the duplicate "Mini-CEX (Paediatrics)" by W-006, which
removed the operator-built `*_paed` types ([T123] d3 had already narrowed the picker by ladder). [T159] stops the
scenario runbook from rebuilding them.

**Current pinning, after W-006 rebuilt dev on 2026-09-20:** 16 of 16 curriculum items pinned (IM Core 1, v11.1 15),
two scales (`O-R Scale` and the CPSA ladder), no `*_paed` types and no FCPaed curriculum (DECISIONS.md W-006). The ids
moved with the rebuild. Inferred from STATE.md ("15 lists on curriculum 2"): on today's dev database the v11.1
curriculum is **curriculum 2**. "Curriculum 2" (FCPaed) and "curriculum 3" (v11.1) in the evidence-run text above, and
in task files written before 2026-09-20, are the pre-rebuild ids. Seeders pin; T109's migration deliberately
backfills nothing.

**Nothing is live.** No real users, only regenerable scenario data. Backward compatibility is not a
constraint anywhere in this programme, and the word should not appear in an argument for a design.

**Production was checked on 2026-09-24** (read-only; recorded in [T157]). It runs the 16 September build: last
migration `20260619065511_T096_AuditDeleteForArchival`, one college (`DEMO-C`), one curriculum (`IM Core`), one user
(the admin), no CPSA types and no activities. Nothing from [T098] on has reached it, and it never held the legacy
`*_paed` rows, so no data decision has to be re-run by hand there. The deploy is [T157]. **The operator has deferred it
until the EPA stream is complete.** Take a `pg_dump` first: [T130]'s migration empties the progress table and the
bootstrapper refills it (STATE.md).

---

## 2. The complete inventory

Re-baselined 2026-09-24. § 2A is the open queue for the EPA stream; § 2B keeps the rows closed since the 2026-09-19
baseline.

Size: **S** = a session or less · **M** = one focused session · **L** = several sessions.
Verdict: **READY** = an implementer can start today · **NEEDS A DECISION** = the named decision (in § 3, or inside the
task file) must land first · **NEEDS DESIGN** = a design pass comes before code.
Sizes and verdicts are the 2026-09-24 survey's for every task it read, except where the register's same-day amendments
settled a decision the survey flagged ([T135]). For tasks filed after the survey ([T158]–[T171]) and the two it did not
read ([T147], [T151]), the size is **inferred** from the task file's "What to build" and marked so.

### 2A. Open

Grouped in the order § 4 recommends. A `+` joins tasks that land in one change; the folded task closes with its host.

| Task | What it is | Verdict | Size | Depends on |
|---|---|---|---|---|
| **Ready now** | | | | |
| [T127] + [T143] + [T148] · P2 | A refused Submit on `/activities/new` leaves an orphan draft and a retry makes another; after a success the form keeps its values ([T143]); Submit cancels a `requested`-born activity at once ([T148]). One change to `CreateOrTransitionAsync`: navigate to `/activities/{id}` after the create | READY | S each, one change | — |
| [T138] · P3 | The committee's evidence snapshot includes draft, open and withdrawn MSF campaigns. Keep `Released` only | READY | S | — |
| [T107] · P2 | An activity pinned to a superseded version still offers a transition it cannot complete. D33 part 1: render it disabled, with a reason naming the fields | READY | S | — |
| [T125] + [T136] · P2 | Curriculum minima are typed as bare integers against an invisible ladder. Rung pickers; a scale change resets them; the refusal names the scale and the value ([T136] folded in) | READY | M | — (sequence with [T139], which edits the same page) |
| [T137] + [T106] item 14 · P2 | A campaign's per-EPA MSF rows are indistinguishable on the trainee's list. Stamp `Activity.EpaId` from a schema pointer; add `ObservedOn` and the credit outcome to the list DTO; fix `/activities/inbox` too | READY | M | — (blocks [T167]) |
| [T135] + [T150] · P2 | The sampling report's denominator and numerator disagree, and it and the trajectory count drafts, cancelled requests and a hard-coded assessor key ([T150], [T106] item 9). Count only a terminal state of the pinned workflow | READY: the state decision is recommended in its 2026-09-24 update; record it as a D-number when built | M | — |
| [T160] · P2 | The encounter date is unbounded. Refuse a future or pre-programme date; warn, never refuse, past D15's fourteen days | READY | M (inferred) | — |
| [T113] · P2 | Six trainee-id queries, and the MSF Open, Close, Withdraw, AddInvitation and aggregate report, take no principal. One shared resolver in Application (widened 2026-09-24) | READY | M | — |
| [T159] · P3 | The paediatric scenario runbook (Acts 1–2) still builds the FCPaed world by hand. Retarget it onto the seeded catalogue and replay it | READY | M (inferred) | — |
| [T140] · P2 | The integration suite's MSF flow test cannot set itself up on a fresh schema, and leaks a schema when it fails | READY | S | — |
| [T141] · P3 | A trainee cannot reach My progress from the navigation | READY | S | — |
| [T142] · P3 | Activity pages print raw user ids where people's names belong | READY | S | — |
| [T144] · P3 | Classify rated evidence by `WbaToolKey` and retire the hard-coded family list | READY | S | — |
| [T145] · P3 | The legacy FormEpaLink admin screen maps instruments to EPAs and restricts nothing | READY | S | — |
| [T147] · P3 | CampaignEdit's "Evidence for these EPAs" label points at no element | READY | S (inferred) | — |
| [T151] · P3 | The assessor nudge job emails deactivated and opted-out nominees | READY | S (inferred) | — |
| [T158] · P3 | Deactivating an EPA takes it out of the picker but not off the progress page, and credit still applies ([T104]'s step 2) | READY | S (inferred) | — |
| [T161] · P3 | An undated activity's filing timestamp is shown as its encounter date (D28) | READY | S (inferred) | — (if [T137] lands first, the list gets the marker too) |
| [T162] · P3 | The type picker offers the system-written `msf_cpsa` to trainees | READY | S (inferred) | — |
| [T163] · P3 | Every MSF response loads and hashes every invitation ever issued | READY | S (inferred) | — |
| **The committee** | | | | |
| [T167] · P2 | The committee's evidence snapshot names no EPA, tool, rating or encounter date, and a STAR can be staged on an EPA outside the trainee's curriculum | READY once its dependency lands | M (inferred) | [T137] |
| [T165] · P2 | A one-member panel validates and the chair alone ratifies. Panel composition, attendance and quorum, the Administrator bypass | NEEDS A DECISION: the quorum floor and the Administrator bypass (recommendations in the task) | M (inferred) | — |
| [T166] · P2 | Nothing compares a trainee's entrustment decisions with Annexure A's year targets or the exit rule | READY, except what recording `Graduate` does when the rule is unmet (operator; recommendation: warn and require a reason) | M (inferred) | — |
| [T168] · P3 | No surface shows, per EPA and period, whether a released MSF covered it | READY | M (inferred) | — (feeds [T167]'s per-EPA group) |
| [T169] · P3 | The portfolio PDF has no per-EPA progress, and prints unrated and MSF evidence as never completed | READY | M (inferred) | — ([T166] and [T168] slot in when they land) |
| [T131] · P2 | Governance ([T098] phase 4): a decision cadence from Annexure B, per-institution panel routing with a neonatal CCC for EPAs 4–5, an EPA agenda on reviews, and Annexure A's `currency` column | NEEDS A DECISION **D38**, then NEEDS DESIGN | L | [T130] (done); [T167] and [T138] before it or with it; College question 6 |
| **Gated instruments and seed fidelity** | | | | |
| [T154] · P3 | Clinical audit (EPAs 1–3) and portfolio and logbook review (EPA 15) cannot be filed | NEEDS A DECISION **D34**, and the portfolio-review shape | M | — |
| [T164] · P3 | Learner feedback (PAED-015) cannot be recorded | NEEDS A DECISION **D35**; the teaching-context threshold NEEDS DESIGN (College question 9) | M (inferred) | — |
| [T170] · P3 | The candidate's milestone self-assessment and learning plan, page 8's fourth information source, have no instrument | NEEDS A DECISION: the College's answer (question 11), or the operator's call to proceed without one | M (inferred) | — |
| [T139] · P3 | The v11.1 items are seeded with a 12-month window, and `max(WindowMonths)` gives a four-year registrar a one-year completion date | NEEDS A DECISION: what `WindowMonths` is (reopens D19; the survey recommends deleting it for an explicit programme length) | M | College question 10; sequence with [T125] |
| **Decide or defer: not required for v11.1** | | | | |
| [T146] · P3 | A crediting activity type from another discipline can credit a trainee's curriculum | NEEDS A DECISION: whether a type's scope bounds what it may credit (to be recorded as D43) | M | — (revisit with D21) |
| [T152] · P3 | A supervisor based at another institution cannot be named on a trainee's assessment | NEEDS A DECISION: build now, or keep D23's documented limit (the survey recommends defer) | M | — |
| [T153] · P3 | A trainee who has left an institution still files there and is shown its staff to nominate | NEEDS A DECISION: the rule (the task recommends refusing at create) | S | — |
| [T171] · P3 | An activity pinned to a superseded version cannot be re-pinned (D33 part 2) | Deferred by D33 part 2; the credit rule on a re-pin NEEDS DESIGN | M (inferred) | — (part 1 is [T107]) |
| **After the stream** | | | | |
| [T157] · P1 | Production runs the 16 September build: nothing from [T098] on is deployed | READY, **deferred by the operator** until the stream is complete | S, operator | `pg_dump` first, then `deploy/deploy.ps1` |

[T106] was triaged on 2026-09-24 and stays a holding file outside the EPA stream: item 14 went to [T137], item 9 to
[T135] through [T150], and items 3, 4, 6, 8, 10 and 12 are struck with pointers. Items 1, 2, 5, 7 and 11, and 13's
residue, remain as platform work. Item 10 (`WindowMonths`) was D19, closed 2026-09-23; its seeding and completion-date
follow-up is [T139].

### 2B. Closed since the 2026-09-19 baseline

| Row as it stood | Now | Evidence |
|---|---|---|
| [T119] — wire `observed_on` | **DONE 2026-09-19** | `CreditApplier` credits off `Activity.ObservedOn`; unblocked [T130] |
| [T123] d1, d3 — axis from the pinned scale; picker narrowed by ladder | **DONE 2026-09-19** | the sequencing warnings with [T110] and [T126] are spent (both shipped). d1 step 2 was not built and has no task (§ 5 item 14) |
| [T123] d2 — two "Mini-CEX (Paediatrics)" and two "DOPS (Paediatrics)" in the picker | **MOOT**; D31 closed 2026-09-24 | the `*_paed` types exist on neither database (W-006; [T157]) |
| [T111] — `?type=` ignored | **DONE 2026-09-19** | — |
| [T100] — rung labels | **DONE 2026-09-19** | the admin editor was split out as [T125] |
| [T099] — the catalogue invisible to non-admins | **DONE 2026-09-24** | production never had the catalogue: [T157] |
| [T105] — transition validation scope | **DONE 2026-09-24** | D22 closed; commit `0a767c2` |
| [T102] fixes 2–3 — `user` field values | **DONE 2026-09-24** | D23 closed; commit `cbc6ca2`. Follow-ups: [T149] (done 2026-09-24), [T150]–[T153] |
| [T110] — `or_scale` binding and the duplicate ladder | **DONE 2026-09-20** | D25 closed by deletion (W-006); commit `b6d594e` |
| [T126] — which ladder an activity was rated on | **DONE 2026-09-20** | full D30 shipped with it; commit `8f23143` |
| [T121] — MSF cannot credit an EPA | **DONE 2026-09-21** | D8–D11 closed 2026-09-20. By D8 MSF credits nothing: a released campaign writes one terminal `msf_cpsa` evidence row per covered EPA, with `counts_for: []`; commit `51d3d8f` |
| [T120] group 1 and the reflective exercise | **DONE 2026-09-24** | `cca_cpsa`, `rca_cpsa`, `chart_stimulated_recall_cpsa`, `reflective_exercise_cpsa`; commit `69fc5d9`. Group 2 was split to [T154] |
| [T122] — the EPA→tool allow-list | **DONE 2026-09-23** | D20 and D21 closed; follow-ups [T144]–[T147] |
| [T130] (was [T098] phase 3) — the annual quota | **DONE 2026-09-23** | D17–D19 closed, D39–D42 decided (§ 3E); commit `256de22` |
| [T098] phase 4 — governance, "no task file" | **Filed as [T131]** | its dependency on phase 3 is met |
| [T104] — retire the legacy FCPaed world | **CLOSED 2026-09-24**, by finding | the data exists nowhere, so D24 and D31 are moot. Successors: [T158] (EPA deactivation), [T159] (the runbook) |
| Rebuild fixes — no caller, not atomic, no stamp | **DONE**, by [T119] (`e115a4f`) and [T130] (`256de22`) | two callers (`CurriculumProgressRebuild.razor:88`, `CurriculumProgressBootstrapper.cs:96`); one `SaveChangesAsync` with an in-memory rollback; stamps `CreditedItemCount` and `CreditScaleMismatchCount` |
| [T106] item 14 — zero-credit completions visible one at a time | **Folded into [T137]** | § 2A |
| MSF defects | **Split.** The dead reminder link: **DONE 2026-09-20** by [T132] (`087dd5f`). The unscoped coordinator list and the unscoped write side: [T113] | Create and Release take a principal since [T121] |
| [T118] — holding file | **CLOSED 2026-09-24** | every finding landed or is moot; its dev evidence rows went with W-006 |
| [T129] — the College message | **DONE 2026-09-20** | all fourteen answered, in two passes (§ 3A-ii) |
| [T134] — the sampling report saw no v11.1 evidence | **DONE 2026-09-20** | commit `50aa464`; this was Wave 3's `GetSamplingConcentrationWarnings` fix |

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
> semester or the second** (see D13). Both are one line from the College: § 3F questions 5 and 1.

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
will look for it, or the mapping stops matching the published table line by line. **Recorded by [T122]:** the
catalogue keeps each EPA's Annexure A cell verbatim as `annexureTools`, its vocabulary lists "Case note review"
under `cca` in `annexureNames` with a note citing D4, and `PaediatricCatalogueToolSeedTests` resolves every cell
into the seeded keys, failing on a name nothing claims.

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
> trainee's entrustment trajectory at all.** Today it does not, deliberately. It is § 3F question 7
> (optional).

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
([T107]; the re-pin is [T171], deferred by D33 part 2), and a rebuild replays each activity against its
**pinned** version — so whatever `msf_cpsa` v1 ships with is permanent for every activity created under v1,
in both directions. "Ship `[]` now and
switch when it settles" was never available. `msf_cpsa` ships `counts_for: []` from v1.

**D12 — Does EPA 7 exclude general Direct observation? — CLOSED 2026-09-20. No, it does not.**
**Consequence: EPA 7's allow-list gains Direct observation**, and `observed_clinical_exam_cpsa` is not
written — the College's first reply said a Mini-CEX and a clinical examination are the same thing, and
EPA 7 already permits Mini-CEX, so a second seed would be a duplicate in the picker. **Applied by [T122]:**
"Directly observed clinical examination" is an alias of `mini_cex` in the vocabulary, and PAED-007's `wbaTools`
carries `direct_observation`, with a `wbaToolsNote` saying the College added it.

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
> 8's definition of Direct observation, not of Mini-CEX. **Ask before merging.** It is § 3F question 5.

**D13 — The academic year and the semester boundary — CLOSED 2026-09-20. January to November, with
the boundary in June.** Eleven months, matching Annexure B. National, as proposed — a per-institution
boundary would give two registrars in the same national programme different targets in the same month.
**Consequence: [T130] is unblocked.** One line is still wanted before it buckets anything: **whether
June itself falls in the first semester or the second.** Recorded here as Jan–Jun / Jul–Nov, which is
the reading of "boundary in June" this register has taken; a June encounter is the only thing that
moves if it is wrong. It is § 3F question 1; D40 fixed the reading in code.

**D15 — A deadline for filing after the encounter? — CLOSED 2026-09-20. A soft warning beyond
fourteen days, and no hard refusal.** Fourteen, not the ninety proposed. **Consequence: this is new
work, not a change.** Verified 2026-09-20 that no observation-date staleness warning exists today —
the only 90-day constants in the repository are the portfolio-export and scheduled-job-run retention
jobs. Fourteen days is tight enough that it will fire routinely, so the "no hard refusal" half is
load-bearing: a registrar blocked by a date validator types today's date instead, which destroys the
encounter date that [T119] exists to protect. **Filed as [T160]** (P2, 2026-09-24), with [T119]'s two
policy-free bounds (not in the future, not before `ProgrammeStartDate`), which were verified unenforced on
2026-09-23 during [T130]: `SchemaValidator.ValidateDateField` checks only that the string parses.

---

### 3B. For the College / CPSA content owner

**All answered.** The first message ([T129], 2026-09-20) carried D1, D4, D6–D16 and D37; page 8 ([T124]) had already
answered D2, D3 and D5. Every one is CLOSED: C1, D2, D3 and D5 in § 3A, the rest in § 3A-ii. The pre-reply text that
stood here (each question with its options and the recommendation sent) is no longer repeated. Each question as sent,
with the College's answers inline, is in `knowledge/college-rfi-v11-1.md`, and this section's full text is in git
history (last at `431e69e`). The next message is § 3F.

Two leftovers from the pre-reply text. D15's recommendation read "a soft 90-day warning"; the College answered
**fourteen days** (§ 3A-ii), and the work is [T160]. **D38 moved to § 3C** on 2026-09-24: it is the maintainer's call,
not a College question, as its own text said.

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
practised" is six months for EPAs 1, 2, 4, 5, 10 and 12 (10 and 12 added 2026-09-24 from `annexure-a.json`),
yet all fifteen are seeded at 12 — filed as [T139], not fixed.
It exists, is validated (`ManageCurriculumItems.cs:41,57`), is admin-editable, is carried through clone —
and is read by nothing in the credit path. Its sole consumer is `AdmitTrainee.cs:126`. An implementer will
find a period-shaped column already there and assume currency is enforced.
*Options:* use it as the period length · leave it and name the new concept differently · delete it.
**Recommendation: leave it, and say so in the phase-3 task.** It is per-item currency, a different question
from the quota period, and quietly repurposing it would give one column two meanings.

**D20 — Where is the EPA→tool allow-list enforced? — CLOSED 2026-09-23 ([T122]), as recommended: (d).**
One predicate, `ToolPermission.Evaluate`, behind the EPA picker and the write path, applied **per credit directive**.
**Every target is checked at create. A changed target is checked on any move from which credit can still be reached,
whoever makes it. An unchanged target is re-checked only when the author hands it on while still able to correct it**:
the mover is the subject or the creator, nobody else has acted yet, the mover can write that directive's field now,
and the move hands it on (credit can follow without coming back through the state it left, or the mover loses write
access to the field). A move from which credit cannot be reached (a CPSA `cancel` or `decline` into a dead end) is
never checked, and a literal `curriculum_item_id` directive is judged only at create. So a pre-T122 draft is refused
when the trainee submits it. An assessor's completion, including an assessor allowed to correct the EPA, is never
refused for an unchanged target, and neither is a trainee's sign-off after assessment or a resubmission after a
decline; an assessor who CREATED the activity is its author. Four review rounds showed why the rule is stated this way:
every test keyed on the shape of an actor rule (`subject` arms, `role:Trainee` submits, fallback approvers, legacy
`requested`-born types, multi-directive rules) broke on some builder workflow. `CreditApplier` and the rebuild never
consult it: dev activity 11, a Mini-CEX
against PAED-011 that its list forbids, is still credited after a rebuild. The refusal is one page-level sentence per
refused item, up to three and then a count of the rest, led by the field's label and naming the instruments the
curriculum accepts, not a message beside the picker, because no per-field error plumbing exists. **Boundaries, recorded:** the gate evaluates the curriculum the subject is on
at the gated write. A subject with no trainee profile passes, and a profile created or re-pointed after the author's
last gated move (normally the submit) is not re-checked when the assessor completes. A list or instrument changed
after the create is applied to an UNCHANGED target only if the author hands the activity on before anyone else acts.
If an assessor picks a draft up first, or the type is born with the assessor (the legacy `requested`-initial shape),
the create was the last check for that target. A changed target is always checked against the current list and key.
One conservative residual is kept on purpose: a withdrawal out of the draft into a holding state the author cannot
edit counts as a hand-on, even if only the author can reopen it; the refusal lands on the author, who can act on it.
*Options:* (a) filter the EPA picker only — cheapest, and unenforced for anything arriving through
`Wombat.Api` or an in-flight draft · (b) reject at credit time — refuses at the one moment nobody can act,
and drops into T108's zero-credit banner whose copy points the reader at their curriculum, which is not
what was wrong · (c) credit but warn — a fourth comparison basis, a new counter, a new banner · (d) reject
at submit, with the picker agreeing.
**Recommendation: (d), with `CreditApplier` deliberately not re-litigating.** An allow-list can be edited
after an encounter was filed; refusing credit retroactively deletes evidence a trainee legitimately
collected under the rule in force at the time. One predicate, two callers — the [T108] shape.

**D21 — What may a tool with no recognised `WbaToolKey` credit? — CLOSED 2026-09-23 ([T122]), as recommended: permissive.**
A null key, a null or empty or unparseable list, and an EPA with no item on the subject's curriculum are all
unrestricted. **The trust boundary this draws:** allow-lists bind the College-seeded instruments, and trust whatever
key an institution declares on its own types. An administrator who leaves a rated type unkeyed, or keys it as another
instrument, escapes the lists, and that is the permissiveness this decision chose, not a defect. The generic
Mini-CEX, DOPS and CBD seeds carry their keys because they are those instruments; ACAT is not a College name and stays
unrestricted. Revisit once institutions have had a release with the builder's "This tool is" picker.
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
**CLOSED 2026-09-24 — the explicit property, with three values, not two.** `all` (the default, and strict),
`owned` (only the `required` fields the mover may write in the state they leave) and `draft` (formats only).
`owned` was the addition: with honest `required` flags on the assessor's fields, a whole-schema check at the
trainee's submit would refuse every submit, so "submit" alone could not deliver the point of the change. All 12 seeds
with transitions were re-authored (no compatibility constraint, W-007) and republished once; a second boot republishes
nothing. *Rejected:* inferring the scope from the target state (the convention family every T122 review round broke);
`none` as a value (a withdrawal carrying a malformed patch would store it).

**D23 — Does [T102] fix 2 land before the ten tools?**
Fix 1 shipped with T070 — a trainee can no longer name *themself* as their own assessor. Nothing yet
validates that a submitted `user`-typed value names someone who holds the required role or is inside the
caller's scope; `SchemaValidator.cs:77` routes `FieldType.User` to plain string validation.
*Options:* fix 2 first · ship the tools and fix 2 after · leave it.
**Recommendation: fix 2 first.** Every one of the ten new tools carries a `user` field, and the fix
generalises to any admin-built tool, which is the platform premise.
**CLOSED 2026-09-24 — fix 2 first, with fix 3.** Recorded with it: the institution judged is the **activity's
stamped one** (the subject's), not the caller's, so the answer never depends on who is looking and there is no
Administrator bypass. An unchanged nominee is judged only at the author's hand-on (D20's clause, shared with T122), so an
assessor's own completion is never refused because they have since lost the role. "Deactivated" means an indefinite
lock or an erasure, not a brute-force lockout. Only the institution is matched, never the nominee's speciality, so an
assessor from another discipline at the same institution can be named (rotations); a `role` may therefore name only a
role whose authority is not bounded by a speciality (not the speciality or sub-speciality admins). A supervisor based at
another institution cannot be named ([T152]). *Rejected:* caller-scoped eligibility (an Administrator would be offered
the whole country, and two viewers of one activity would see different lists); re-judging stored nominees on every move
(strands in-flight work, the T122 lesson).

**D24 — [T104]: what happens to the five trainees on curriculum 2? — CLOSED 2026-09-24, moot.**
No curriculum-2 (FCPaed) trainee exists anywhere: W-006 removed them from dev on 2026-09-20, and production never
had them ([T157]). [T104] closed by finding; its surviving pieces are [T158] and [T159]. The question as it stood:
*Options:* re-pin them to curriculum 3 — needs curriculum 2 pinned to its scale first and a documented
ordinal remap, because a legacy "4 = Independent" silently becomes v11.1's "4 = 3b" · leave them to finish
on curriculum 2 and apply the national catalogue to new intakes only.
**Recommendation: leave them.** A registrar mid-programme should not have their assessment history
reinterpreted, and the alternative is a hand-run live migration over 15 `EntrustmentDecisions` and 4
progress rows with no code to replay it on production. Choosing this also removes T104's only hard blocker
and lets the legacy world be retired by deactivation rather than by migration.

**D25 — [T110]: are "O-R Scale" and "Paed General Entrustment Scale" the same ladder? — CLOSED 2026-09-20, by
deletion ([T110]'s amendment; marked here 2026-09-24).** W-006 removed "Paed General Entrustment Scale", which was
operator data. Two scales remain, `O-R Scale` and the CPSA ladder, and the generic seeds bind by exact name
(`"scale_key": "O-R Scale"`). The stable slug stays unbuilt, with no task. Inferred: it is not needed, because
renaming a scale is refused while a published schema names it (`UpdateEntrustmentScaleCommandHandler.cs:42-43`).
The ⚠ below is spent: [T110] and [T123] d1 both shipped. The question as it stood:
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

**D38 — Does a committee decision have to record what it was grounded in? — OPEN; gates [T131].** *(new 2026-09-19,
[T124]; moved here from § 3B on 2026-09-24)*
Page 4 of the source is unambiguous: *"The summative entrustment decision for an EPA is taken by the
Clinical Competency Committee, drawing on the standard assessment information sources set out on page 8 —
**never by a single assessor and never from a single form**."* Wombat today lets a committee record an
entrustment decision with **no stated evidence basis at all**. Observed at `431e69e`: every STAR staged through the UI
carries zero evidence links, because `ReviewDetail.razor:427` passes an empty array and the validator requires none
(`StagePendingEntrustmentDecision.cs:45-49`).
*Options:* (a) a phase-4 requirement: each staged decision or agenda line names at least one item from the frozen
evidence snapshot · (b) guidance only · (c) nothing.
**Recommendation: (a), written into [T131]**, whose 2026-09-24 update records it. It is the College's own words about
what a decision *is*, not a product preference, and it is the maintainer's call on scope, not a College question. It
costs less than it looks: `EntrustmentEvidenceLink`, `PendingEntrustmentDecision.EvidenceLinksJson` and the
materialisation at ratify (`RatifyCommitteeDecision.cs:70`) already exist, so the work is a picker over the snapshot and
a `NotEmpty` rule. The links draw from the snapshot, so [T167] and [T138] land before the agenda or with it. The "never
by a single assessor" half is [T165].

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
undeserialized, left for [T131] with a named entry in a guard test ([T131]'s 2026-09-24 update takes it up;
what the column means is § 3F question 10). *Rejected:* `currency` (breaks Annexure B's arithmetic). **Contested, and with the College (§ 3F):** EPAs 3, 6 and 7's own pages say "performed annually", so
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

Questions 1–5 were collected on 2026-09-23 and 2026-09-24. Questions 6–11 were added on 2026-09-24 from the EPA-stream
survey and the task register; 8–10 were derived by the survey, not raised by the College or an earlier message. None
blocks a build. 1–4 are a read-model change, one constant or a rebuild, because storage is per semester. 5 and 8 are
each a catalogue edit plus a new migration. 6 and 10 shape [T131] and [T139]; T131 builds per-institution routing and
cadence only in the meantime. 9 shapes [T164], which reports the count and does not gate on it until answered. 7 is
optional. 11 gates [T170], which does not invent the form. They are listed so they go in one message.

1. **June** (D13): is June itself in semester 1 or semester 2 — and is the boundary a date in mid-June?
2. **December** (D40): which period does an encounter observed in December count towards?
3. **Per-semester figures** (D39): are Annexure B's per-semester figures a target *each* semester, or a planning
   split of the annual frequency? EPAs 3, 6 and 7's own pages say "performed annually".
4. **Late starters** (D42): is a registrar who starts in the first days of a period (e.g. the first working day of
   January) exempt for it? And is a registrar who finishes part-way through a period held to its target?
5. **"Clinical observed interaction"** (D12, [T122]): does the proposed merge join Mini-CEX with **Direct
   observation**? If so, eight EPAs' tool lists change, including PAED-010 ("Leading and operating within a clinical
   team"), which would become creditable by a Mini-CEX. [T122] seeded the two as separate instruments. The answer is a
   catalogue edit plus a new migration for existing databases.
6. **Committee structure** ([T131]): is the Clinical Competency Committee arrangement, including a neonatal CCC for
   EPAs 4–5, set per institution, or mandated nationally by the College?
7. **MSF on the trajectory** (D36, optional): should the level a releasing reviewer records on an MSF appear on a
   trainee's entrustment trajectory, or stay evidence only? Today it does not, deliberately.
8. **Chart-stimulated recall** ([T120], [T124]): is it the same instrument as Clinical Case Analysis? [T124] read it as a
   strong alias, but it was never asked, and [T120] seeded it separately. A yes adds CCA to EPA 12 and chart-stimulated
   recall to EPAs 1, 3, 4 and 6.
9. **Learner feedback** (D35, [T164]): is it multi-source feedback from students, junior trainees and team members? And is
   EPA 15's "at least two assessors across at least two teaching contexts" two separate occasions, or two respondent
   groups?
10. **"Currency / status"** (D39, [T139], [T131]): is Annexure A's column an expiry, so that an entrustment lapses if the
    EPA is not practised for 6 or 12 months, or the entrustment-decision cadence Annexure B relabels it as?
11. **The candidate's self-assessment** ([T170]): is there a prescribed form for page 8's milestone self-reflection and
    learning plan, how often is it written, and who signs it off?

### 3D. Decisions inside one task

The recommendation stands unless overruled. The options and their costs are written out in the task file
named in the last column — these are here so the list is complete, not so they are re-argued.

| # | Question | Recommendation | Written up in |
|---|---|---|---|
| D26 | Do the five undated generic WBA seeds (`mini_cex`, `dops`, `cbd`, `acat`, `teaching_session`) gain a required `observed_on`? | Yes. An assessment without an encounter date is not evidence of anything | [T119] D1 |
| D27 | Does the encounter date drive the portfolio PDF, the committee review window and the sampling warnings, or only credit and the trajectory? | All of them. Two dates in one product with no rule for which is which is the ambiguity T119 exists to remove | [T119] D2 |
| D28 | Is a `CreatedOn`-sourced date visibly marked to the reader? | Yes, as a follow-up behind T100 — it reads better once the neighbouring label defects are fixed. **[T100] shipped; the follow-up is filed as [T161]** (2026-09-24) | [T119] D4, now [T161] |
| D29 | Rung labels on a 40px axis margin: label or ordinal? | Label when ≤4 characters, ordinal otherwise; full label in the tooltip and the screen-reader table **unconditionally**, so the accessible copy is never poorer than the picture | [T123] D1a |
| D30 | Points on a different ladder from the axis? | Keep them, draw them hollow, exclude from the polyline, label them with their own scale's rung. Dropping them erases recorded evidence the trainee did not create. **Applied by [T126]** (2026-09-20) | [T123] D1b |
| D31 | The duplicate-name rename: which four get renamed? | **CLOSED 2026-09-24, moot.** The legacy `*_paed` types exist on neither database (W-006; [T157]), and no two seeded types share a name (`ActivityTypeSeedCatalogue.cs:94-157`). The recommendation had been the legacy four | [T123] D2 |
| D32 | Should the default rung rendering be the label alone? | Yes. The College's own document says "3a", never "level 4". Keep the ordinal only in the admin scale editor, in its own column | [T100] |
| D33 | [T107]: stranded activities — suppress the impossible action, allow a re-pin, or migrate the handful? | **Part 1 decided: suppress.** The recommendation stood, restated after [T105] in [T107]'s 2026-09-24 update: render the action disabled with a reason naming the fields, not hidden. **Part 2, re-pin vs migrate: recommendation (b), defer**, filed as [T171] (P3, pre-launch readiness). Migrating is moot: production has no activities and W-006 emptied dev | [T107], [T171] |
| D34 | File attachments: build storage, or accept a URL field for now? | **OPEN; gates [T154]'s `clinical_audit_cpsa`.** Recommendation: a `text` field holding a link now; attachments as their own task. An attachment is personal data — it has to appear in `AccessReportBuilder`'s subject-access export, be handled by `ErasureExecutor`, survive the portfolio PDF and pass the CSP. It should not arrive as a side effect of a seed folder | [T120] D7, now [T154] |
| D35 | Learner feedback: an MSF template, or a separate token-based form? | **OPEN; gates [T164].** Recommendation: an MSF template with a new `Learner` value on `MsfRespondentCategory`. It reuses anonymity, thresholds and token issuance that would otherwise be rebuilt badly. Flag that EPA 15's "two assessors across two teaching contexts" is a per-*context* threshold `MinimumCategoryResponses` cannot express; how the College counts it is § 3F question 9 | [T120] D8, now [T164] |
| D36 | Should MSF plot on the trajectory chart? | Not under [T121]'s design. `TryParseObservation` requires an `assessor_user_id` and MSF has no honest value for it; naming the release reviewer "the assessor" would be a lie on a clinician-facing chart. Revisit with the "Evidence type" rename. Optional College question: § 3F question 7 | [T121] adjacent 5 |
| D44 | [T135]/[T150]: which activities count as sampled rated evidence, and who is the assessor? | **Decided 2026-09-24.** A terminal state of the **pinned** workflow (where credit fires): `completed` for the rated seeds, `recorded` for `msf_cpsa`. The EPA is the stamped `Activity.EpaId` ([T137]). The rating is the pinned `rated_level_field`. The assessor is whoever the rating's `field:` rule names, else the `field:` actors into a terminal state, else the nominee fields when a role writes it, else nobody. Every row lands in exactly one count: attributed, withheld, unreadable (incomplete) or not attributed (MSF; not incomplete). *Rejected:* literal `completed`, "completed or rated", credited-only, folding unreadable into withheld, first nominee when the trainee rates themselves | [T135] as-built |

---

## 4. The waves

Each wave is a set that can run in parallel. The order between waves is a real dependency, not a
preference, except where it says otherwise. **Re-baselined 2026-09-24:** Waves 0–4 are done and Wave 6 is moot. The
waves are kept as the record of how the stream was sequenced. What is left, and in what order, comes first.

### Order to finish the stream (2026-09-24)

The operator's instruction stands: *"Not deploying yet, we need to get the EPA stream completed."* So [T157] deploys
after this list, not during it. W-007 applies throughout. The order is a recommendation drawn from the survey; the only
hard arrows are the "Depends on" column in § 2A.

1. **Ready work first.** The [T127] group ([T127], [T143] and [T148], one change to `NewActivity.razor`), [T138],
   [T107], [T125] (with [T136]), [T137] (with [T106] item 14), [T135] (with [T150]), [T160] (the encounter-date bounds),
   [T113], then the small ones: [T140], [T141], [T142], [T144], [T145], [T147], [T151], [T158], [T161], [T162] and
   [T163]. [T159], the runbook replay, can run beside them.
2. **Then the committee work.** [T167] (the evidence snapshot) once [T137] has stamped `Activity.EpaId`; [T165] (who
   takes a decision, and the quorum); [T166] (year targets and the exit rule); then [T168] and [T169]. [T131] comes
   last, once D38 is decided and its design pass is done. College question 6 informs its routing but does not block it.
3. **Then the decision-gated instruments.** [T154] on D34 and the portfolio-review shape; [T164] (learner feedback) on
   D35; [T170] on the College's answer to question 11.
4. **[T139] with the currency question** (§ 3F question 10), decided alongside [T131]'s cadence. It edits the same page
   as [T125], so it follows it.
5. **Decide or defer; none is needed for v11.1:** [T146] (D43), [T152], [T153], [T171].
6. **Then [T157] deploys**, with a `pg_dump` first.

Operator decisions to take along the way: D38 (gates [T131]), D34 and the portfolio-review shape (gate [T154]), D35
(gates [T164]), [T165]'s quorum floor and Administrator bypass, [T139]'s `WindowMonths`, [T166]'s graduation
behaviour, and whether to build or defer [T146], [T152] and [T153].

### Wave 0 — the evidence run — **DONE (2026-09-19)**

One real assessment end to end on curriculum 3. It proved the credit engine and produced eight findings,
three of them new. Everything below is planned against evidence rather than against a reading of the code,
which was the point.

### Wave 1 — ask, extract, and fix what needs nobody — **DONE**

Four independent threads, all startable today.

1. ~~**Send the College D1, D4, D6–D16 and D37.**~~ **DONE 2026-09-20 — [T129].** All fourteen were answered, in two
   passes (§ 3A-ii). The next message is § 3F.
2. ~~**Extract page 8 (D2).**~~ **DONE 2026-09-19 — [T124].** It closed D2 (the answer is *no*: page 8
   defines nine tools in one sentence each and four *information sources*, and specifies no field sets),
   closed D3 (CCA = **Clinical Case Analysis**) and D5 (RCA = **Random Case Analysis**), and evidenced D4
   and D12: the document **defines nine tools and Annexure A uses fourteen**, with a clean partition —
   every tool on ≥3 EPAs is defined, every tool on ≤2 is not. It also extracted **Annexure B for the first
   time** (`T098-data/annexure-b.json`), which hands phase 3 a published per-semester column and phase 4 a
   decision cadence, and it found a **source contradiction about MSF** (D37).
3. ~~**[T119] — `observed_on`.**~~ **DONE.** `Activity.ObservedOn` is a real column and credit runs off
   it; `ResolveObservationDate` is gone. The rebuild warning that stood here is spent.
   `RebuildCurriculumProgressCommand` now has two callers (`/admin/curriculum-progress`,
   `CurriculumProgressRebuild.razor:88`, and `CurriculumProgressBootstrapper.cs:96`), commits in one `SaveChangesAsync`
   with an in-memory rollback, and stamps `CreditedItemCount` and `CreditScaleMismatchCount`. Fixed by [T119] and
   [T130] (observed at `431e69e`). The original instruction is in git history.
4. ~~**The clinician-facing reading of every number.**~~ **DONE 2026-09-19**, browser-verified on dev.
   [T100] tiers 1–3 (premise rewritten first — it was wrong twice), [T123] d1 (axis from the scale),
   [T123] d3 (picker narrowing — the SQL check passed: the four `_paed` types declare `scale_key` **"2"**,
   a raw id, and curriculum 3 pins scale 3, so all four drop off a v11.1 trainee's menu, which is more
   than the task predicted) and [T111]. [T099] closed on 2026-09-24: production never had the catalogue, and
   the deploy is [T157], deferred.
   Three defects in the specified fixes were caught before they shipped — an HTML-injection hole in D29's
   axis label, an integer-stepper assumption about contiguous ordinals, and `ChartPoint.Label` not being a
   rung label at all. Split out: [T125], [T126], [T127].

**Unblocks:** phase 3 (which cannot bucket without a date), and a registrar being able to read their own
progress page.

### Wave 2 — make the platform ready for ten more seeds — **DONE except [T107]**

**This wave exists for one reason: [T105] must precede the ten tools.** The four CPSA seeds encode a
workaround forced by Submit-mode-on-every-transition. Write five more seeds first and it is baked into nine
of fourteen types, each then needing a re-authored schema, an inverted test and a republish that strands
in-flight activities. It did: [T105] landed (`0a767c2`) before [T120] (`69fc5d9`), and all 12 seeds with
transitions were re-authored with an explicit `validation` (D22).

Runs in parallel: ~~[T105] (D22)~~ **DONE 2026-09-24** · ~~[T102] fix 2 (D23)~~ **DONE 2026-09-24** · ~~[T110] + the
ladder merge (D25)~~ **DONE 2026-09-20** · [T107]'s suppression (D33 part 1): **open, READY** · [T106] item 14: **open,
folded into [T137]**.

The counterweight that stood here (do not block Wave 3 on T105 indefinitely) is spent: [T105] did not slip.

### Wave 3 — fidelity: the instruments

Three independent tracks. Start each as its decisions land.

- **[T121] — MSF. DONE 2026-09-21.** D8–D11 closed. By D8 it credits nothing: a released campaign writes one terminal
  `msf_cpsa` evidence activity per covered EPA, with `counts_for: []`. The dead reminder link that stood on its
  critical path (`MsfInvitationExpiryReminderJob.cs:57` mailed the token hash) was fixed first, by [T132] (2026-09-20).
- **[T120] group 1 — DONE 2026-09-24.** Three rated seeds (D4 and D12 retired two of the five), plus
  `reflective_exercise_cpsa`. The `GetSamplingConcentrationWarnings.cs` blindness to the paediatric keys, named here as
  part of the same change, was fixed separately by [T134] (2026-09-20).
- **[T122] — the EPA→tool allow-list. DONE 2026-09-23.** D20 and D21 closed as recommended. Composes with [T123] d3;
  both keep the permissive fallback, and the EPA picker falls back to T108's creditable set rather than emptying.
  **Consequence for [T120]:** each new `*_cpsa` seed must declare its vocabulary key in `ActivityTypeSeedCatalogue`
  (the entry's `WbaToolKey` is required), and a seed added after T122 reaches an existing database's key only on
  create.
- **Still open in this wave:** [T154] (clinical audit and portfolio review; D34 and a design) and [T164] (learner
  feedback; D35). Between them they cover the three vocabulary instruments without a seed. [T170] (the candidate's
  self-assessment) is page 8's fourth information source, not a vocabulary instrument, and waits on the College.

**State the priority honestly:** none of the ten tools unlocks an EPA. Every one of the 15 already permits
at least one seeded tool. The value here is tool-mix fidelity and the College's own allow-list — not
reachability. If only one thing in this wave gets done, it is MSF. MSF was done first.

**Unblocks:** a portfolio that looks like the one v11.1 describes, and a CCC with case-analysis and
reflective evidence to weigh.

### Wave 4 — the quota ([T098] phase 3) — **DONE (2026-09-23), as [T130]**

Shipped as its own task file. Progress is stored per semester and read against a per-window target through one
shared read model; the College's D14 is applied when reading; the progress page, the trainee dashboard and the
three staff dashboards all say "n of m" for a named period. The instruction that stood here — seed the period from
the `currency` string — was wrong, and is corrected by D39 (§ 3E). The migration empties the progress table on
every existing database, and `CurriculumProgressBootstrapper` rebuilds it at the next startup; the manual rebuild
is `/admin/curriculum-progress`.

### Wave 5 — governance ([T098] phase 4) — **filed as [T131]**

Per-EPA panel routing so EPAs 4–5 go to the neonatal CCC; semester cadence; an EPA agenda on reviews so
"6 decided each semester + 9 annually" can be scheduled and chased. Depends on Wave 4's period concept, which has
shipped. ~~Also has no task file.~~ **[T131]** (P2) waits on D38 (§ 3C), a design pass, and College question 6 for
routing. Its 2026-09-24 update adds Annexure A's `currency` column (D39). Three siblings were filed the same day from the
survey: [T165] (one person can take a decision), [T166] (year targets and the exit rule) and [T167] (the evidence
snapshot). With [T138], they land before T131's agenda or with it.

### Wave 6 — retire the legacy world — **no longer real work (2026-09-24)**

There is no legacy world left to retire. W-006 removed the four `*_paed` types, the FCPaed curriculum and its trainees
from dev on 2026-09-20, and production never had them ([T157]'s read-only query, 2026-09-24). [T104] closed by finding,
and D24 and D31 are moot. Two pieces outlived the data: [T158] (deactivating an EPA hides nothing from progress, and
credit still applies) and [T159] (the scenario runbook still builds the FCPaed world by hand, so a replay would bring it
back). The plan that stood here (deactivation, a hand-run production migration and a runbook) is in git history.

---

## 5. What is still not planned, even now

Honest list. **Re-baselined 2026-09-24:** twelve items stood here on 2026-09-19 with no task file. Most now have one,
and each line says where it went. Items 13 and 14 are new. What is still unplanned is items 2, 3, 11, 13 and 14.

1. ~~**[T098] phase 4 has no task file of its own.**~~ **Filed as [T131]**, with siblings [T165], [T166] and [T167]
   (2026-09-24). What stays open is its design pass, which T131 requires before code, and D38.
2. **File attachments.** Still unplanned. D34 parks it behind a link field, and [T154] carries only the decision.
   Observed at `431e69e`: `FieldType.File` renders a placeholder (`ActivityForm.razor:99-102`), is validated as a plain
   string (`SchemaValidator.cs:82`), and the builder still offers it (`ActivityTypeEdit.razor:340`); no `InputFile`,
   `IBrowserFile` or `IFormFile` exists in `src`. If D34 picks storage, it needs its own task: the data-rights export,
   erasure, the PDF and the CSP. If not, hiding the field type from the builder is a small task the survey suggested
   and nobody has filed.
3. **Portfolio and logbook review needs a design, not a form.** It reviews *"teaching sessions, journal
   club presentations, feedback received, and reflective entries"* — a body of work Wombat already holds. It is now the
   second decision on [T154]. The recommended shape is a review record naming the trainee and a period, pointing at the
   existing portfolio export, signed by a named reviewer. The design itself is not done.
4. ~~**Learner feedback.**~~ **Filed as [T164]** (2026-09-24); it waits on D35. The per-context threshold is § 3F
   question 9.
5. ~~**A tool-mix rule.**~~ **Moot.** D8 closed as (c): MSF credits nothing, and Annexure A has no per-tool sub-quota,
   so there is nothing to cap.
6. ~~**A `SystemManaged` flag on `ActivityType`.**~~ **Filed as [T162]**, which also decides the dead
   `ActivityPermissionRule` table.
7. ~~**The MSF feature's own scope defects.**~~ **In [T113]**, widened 2026-09-24 to the coordinator list and to Open,
   Close, Withdraw, AddInvitation and the aggregate report. Create and Release take a principal since [T121].
8. **The two tool taxonomies are still two.** Moving the classification onto `WbaToolKey` is [T144] (READY, S).
   Nothing forces it.
9. ~~**The 78 descriptors.**~~ **Closed by D16**: narrative, as carried in `Epa.RequiredKnowledgeSkills`.
10. ~~**Production.**~~ **Verified 2026-09-24 and filed as [T157].** The operator has deferred the deploy until the
    stream is complete.
11. **Grouped display of the synthetic MSF activities.** [T137] makes each row say its EPA and encounter date. Grouping
    a campaign's rows stays unbuilt and unplanned.
12. ~~**Nothing plans how the 21 CCC decisions per registrar per year get scheduled or chased.**~~ **[T131]**'s agenda,
    with [T166] for each trainee's standing against the year targets.
13. **The MSF questionnaire** *(new 2026-09-24)*. No College MSF questionnaire is seeded. A template can be made only
    inline on `CampaignEdit.razor:241-256`, as one scale question and one long-text question with no scale bound
    (observed). Page 8 specifies no fields (D2), so this is a College commissioning ask, not code.
14. **[T123] d1 step 2** *(new 2026-09-24)*: deriving the trajectory axis from the activities' own ladders when an item
    is unpinned. It was not built ([T126]) and has no task. Every seeded item is pinned (16/16 on dev after W-006), so
    it is left unplanned on purpose. Revisit only if an institution adopts an unpinned curriculum.

---

## Task files referenced

Every task id in brackets above, by lane, as of 2026-09-24. Paths are under `execution/tasks/`.

**`done/`:** [T070] `T070-assessor-rating-edit-and-note.md` ·
[T098] `T098-epa-v11-adoption.md` ·
[T099] `T099-paediatric-catalogue-unreachable.md` ·
[T100] `T100-entrustment-rung-label-display.md` ·
[T102] `T102-assessor-self-assignment-escalation.md` ·
[T104] `T104-retire-legacy-paediatric-data.md` ·
[T105] `T105-transition-validation-scope.md` ·
[T108] `T108-scope-without-adoption-silently-uncredited.md` ·
[T109] `T109-cross-scale-credit-comparison.md` ·
[T110] `T110-or-scale-key-mismatch.md` ·
[T111] `T111-new-activity-type-query-parameter-ignored.md` ·
[T118] `T118-v11-1-evidence-run-findings.md` ·
[T119] `T119-wire-observed-on-as-the-encounter-date.md` ·
[T120] `T120-remaining-v11-1-wba-tools.md` ·
[T121] `T121-msf-cannot-credit-an-epa.md` ·
[T122] `T122-enforce-epa-tool-mapping.md` ·
[T123] `T123-evidence-run-ui-defects.md` ·
[T124] `T124-page-8-extraction-findings.md` ·
[T126] `T126-an-activity-does-not-know-which-ladder-it-was-rated-on.md` ·
[T129] `T129-send-the-college-the-v11-1-decision-list.md` ·
[T130] `T130-the-annual-quota.md` ·
[T132] `T132-msf-dead-reminder-link-and-unscoped-campaign-list.md` ·
[T134] `T134-committee-sampling-reports-zero-rated-evidence-for-every-v11-1-trainee.md` ·
[T149] `T149-the-sso-link-endpoint-is-an-unthrottled-password-oracle-that-binds-any-external-identity-and-sso-sign-in-ignores-lockout.md`

**`queued/`:** [T106] `T106-activity-platform-backlog.md` ·
[T107] `T107-stranded-activities-pinned-to-old-versions.md` ·
[T113] `T113-caller-supplied-trainee-id-queries.md` ·
[T125] `T125-curriculum-item-minima-are-unguided-integer-writes.md` ·
[T127] `T127-a-failed-submit-leaves-an-orphan-draft-behind.md` ·
[T131] `T131-epa-governance.md` ·
[T135] `T135-sampling-denominator-numerator-disagree.md` ·
[T136] `T136-curriculum-scale-change-fails-silently.md` ·
[T137] `T137-a-campaign-s-per-epa-evidence-rows-are-indistinguishable-on-the-trainee-s-own-activity-list.md` ·
[T138] `T138-the-committee-evidence-snapshot-includes-draft-open-and-withdrawn-msf-campaigns.md` ·
[T139] `T139-the-v11-1-items-are-seeded-with-a-12-month-window-and-max-windowmonths-gives-a-four-year-registrar-a-one-year-completion-date.md` ·
[T140] `T140-the-integration-suite-s-msf-flow-test-cannot-set-itself-up-on-a-fresh-schema-and-leaks-a-schema-every-time-it-fails.md` ·
[T141] `T141-a-trainee-cannot-reach-my-progress-from-the-navigation.md` ·
[T142] `T142-activity-pages-print-raw-user-ids-where-people-s-names-belong.md` ·
[T143] `T143-after-a-successful-submit-the-new-activity-form-keeps-every-value-inviting-a-duplicate.md` ·
[T144] `T144-classify-rated-evidence-sources-by-wbatoolkey-and-retire-the-hard-coded-activity-family-list.md` ·
[T145] `T145-the-legacy-formepalink-admin-screen-maps-instruments-to-epas-and-restricts-nothing.md` ·
[T146] `T146-a-crediting-activity-type-from-another-discipline-can-credit-a-trainee-s-curriculum.md` ·
[T147] `T147-campaignedit-s-epa-checkbox-group-has-a-label-pointing-at-no-element.md` ·
[T148] `T148-submit-on-activities-new-cancels-a-requested-born-activity-the-moment-it-is-created.md` ·
[T150] `T150-sampling-and-trajectory-count-assessors-from-drafts-and-cancelled-requests-by-a-hard-coded-field-key.md` ·
[T151] `T151-the-assessor-nudge-job-emails-deactivated-and-opted-out-nominees.md` ·
[T152] `T152-a-supervisor-based-at-another-institution-cannot-be-named-on-a-trainee-s-assessment.md` ·
[T153] `T153-a-trainee-who-has-left-an-institution-still-files-new-activities-there-and-is-shown-its-staff-to-nominate.md` ·
[T154] `T154-clinical-audit-and-portfolio-review-cannot-be-filed-one-needs-an-attached-report-the-other-a-design.md` ·
[T157] `T157-production-runs-the-16-september-build-none-of-t098-t149-is-deployed-so-the-epa-catalogue-does-not-exist-there.md` ·
[T158] `T158-deactivating-an-epa-does-not-hide-it-from-progress-and-credit-still-applies-to-it.md` ·
[T159] `T159-retarget-the-paediatric-scenario-runbook-scenario-paediatrics-md-acts-1-2-onto-the-seeded-v11-1-catalogue.md` ·
[T160] `T160-the-encounter-date-is-unbounded-a-future-or-pre-programme-date-is-accepted-and-nothing-warns-past-d15-s-fourteen-days.md` ·
[T161] `T161-an-undated-activity-s-filing-timestamp-is-presented-as-its-encounter-date.md` ·
[T162] `T162-the-type-picker-offers-the-system-written-msf-cpsa-to-trainees.md` ·
[T163] `T163-every-msf-response-scans-every-invitation-ever-issued.md` ·
[T164] `T164-learner-feedback-paed-015-cannot-be-recorded.md` ·
[T165] `T165-a-committee-decision-can-be-taken-by-one-person-a-one-member-panel-validates-and-the-chair-alone-ratifies.md` ·
[T166] `T166-the-committee-cannot-see-where-a-trainee-stands-against-annexure-a-s-target-level-for-their-year-or-against-the-exit-rule.md` ·
[T167] `T167-the-committee-s-evidence-snapshot-names-no-epa-tool-rating-or-encounter-date-and-a-star-can-be-staged-on-an-epa-outside-the-trainee-s-curriculum.md` ·
[T168] `T168-no-surface-shows-per-epa-and-period-whether-an-msf-covered-it.md` ·
[T169] `T169-the-portfolio-pdf-has-no-per-epa-progress-and-prints-unrated-and-msf-evidence-as-never-completed.md` ·
[T170] `T170-the-candidate-s-milestone-self-assessment-and-learning-plan-page-8-s-fourth-information-source-have-no-instrument.md` ·
[T171] `T171-an-activity-pinned-to-a-superseded-version-cannot-be-re-pinned-to-the-current-one.md`

Source data extracted from `EPA version 11.1.docx`: `execution/tasks/done/T098-data/annexure-a.json` ·
`execution/tasks/done/T098-data/annexure-b.json` (new, [T124]) · `execution/tasks/done/T098-data/page-8-wba-tools.json` (new, [T124]) ·
`execution/tasks/done/T098-data/epa-detail.json`.
