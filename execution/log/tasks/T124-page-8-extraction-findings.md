---
id: T124
title: "What page 8 actually says, and what Annexure B was hiding"
status: done
priority: P3
created: 2026-09-19
---
# T124 — What page 8 actually says, and what Annexure B was hiding

**Status:** done (findings file — the decisions it closes are recorded in `Programme/EPA-PROGRAMME.md` §3)
**Surfaced:** 2026-09-19, executing the programme's Wave 1 item 2 (decision **D2**).
**Severity:** n/a — this is evidence, not a defect. But it closes three decisions, reshapes two more, and
removes work from [T120].

The programme called extracting page 8 *"the cheapest action in the programme"* and predicted it *"may
collapse D3, D4, D5 and D6"*. It did better than that in one direction and worse in another, and it turned
up a source contradiction nobody had seen.

Extracted data now lives beside the annexures:

- `T098-data/page-8-wba-tools.json` — the nine tool definitions and the four information sources.
- `T098-data/annexure-b.json` — **new**; Annexure B had never been extracted at all.

---

## 1. D2 — the answer is **no**, and the question was subtly wrong

Every EPA's `assessment` field reads *"The standard set applies to this EPA — see 'Standard assessment
information sources' on page 8"*. The programme asked whether that section **defines the instruments**.

It does not, and it was never going to, because page 8 holds **two separate sections** and the EPAs point at
the second one:

1. **"Workplace-based assessment (WBA) tools"** — an abbreviations table. Nine tools, one sentence each.
   This is the part with instrument content in it.
2. **"Standard assessment information sources"** — what the EPAs actually cite. Four items, and they are
   not instruments at all:

   > Clinical documentation — notes, referrals, discharge summaries and prescriptions.
   > Colleagues — consultants, registrars, peers and members of the multidisciplinary team, including
   > nursing and allied health staff and support services such as radiology and laboratory services.
   > Patients, guardians and family members — their experience of the care provided.
   > The candidate — self-reflection on the milestones achieved, and learning plans that promote
   > progressive milestone achievement.

   > *"The same set of information sources grounds the summative entrustment decision for every EPA in this
   > document. It is therefore set out once here rather than repeated in each EPA."*

So the repeated `assessment` sentence is a **triangulation requirement on the committee**, not a form spec.
That reading is confirmed by page 4:

> *"The summative entrustment decision for an EPA is taken by the Clinical Competency Committee, drawing on
> the standard assessment information sources set out on page 8 — **never by a single assessor and never
> from a single form**."*

**Consequences.**

- **D2 closes: page 8 does not specify field sets.** No per-tool rating scale, no required/optional
  structure, no field lists. The form designs still have to be asked for, so D6 and the [T120] field-set
  questions survive intact. The programme's instruction *"do not commission form designs before reading the
  page every EPA points at"* was right to insist, and the page having now been read, the commissioning ask
  is unavoidable.
- **This is a phase-4 requirement, stated by the College, that no task file carries.** "Never from a single
  form" is a rule about what a CCC decision must be grounded in, and Wombat currently lets a committee
  record an entrustment decision with no stated evidence basis at all. Filed as **D38** below.

## 2. D3 closes — CCA is **Clinical Case Analysis**

> **CCA** — *Clinical Case Analysis — review of clinical documentation and discussion of the reasoning and
> management plan recorded.*

The programme guessed the semantics right and the expansion wrong. It recommended *"case-based **c**linical
**a**ssessment (a rated discussion with the notes in front of both parties, reconciling the two)"*. The
reconciling reading is exactly what the College defines — documentation review **and** discussion of the
recorded reasoning, one instrument — but the name is *Clinical Case Analysis*.

**Seed `cca_cpsa`, display "Clinical Case Analysis (Paediatrics)".** The two conflicting per-EPA
descriptions ([T120]'s worry: EPAs 1 and 3 read as a case discussion, EPA 2 as a notes audit) are both
instances of the one definition, so no split is needed.

## 3. D5 closes — RCA is **Random Case Analysis**

> **RCA** — *Random Case Analysis — review of cases selected at random from the trainee's records to
> identify knowledge gaps.*

Confirms the recommendation verbatim: key `rca_cpsa`, display name spelled out. Nobody will read it as root
cause analysis.

## 4. D4 and D12 — the document defines nine tools; Annexure A uses fourteen. The split is clean.

Page 8 defines nine. Annexure A names fourteen. Partitioning Annexure A's fourteen by whether page 8 defines
them gives a result with **no overlap at all**:

| Defined on page 8 | EPAs using it | | Defined nowhere in the document | EPAs using it |
|---|---|---|---|---|
| MSF | 15 | | Chart-stimulated recall | 2 |
| CBD | 12 | | Case note review | 1 |
| Mini-CEX | 9 | | Directly observed clinical examination | 1 |
| DOPS | 8 | | Learner feedback | 1 |
| Direct observation | 8 | | Portfolio and logbook review | 1 |
| CCA | 4 | | | |
| Reflective exercise | 4 | | | |
| RCA | 3 | | | |
| Clinical audit | 3 | | | |

**Every tool used on three or more EPAs is defined. Every tool used on one or two is not.** The minimum
usage among the defined nine is 3; the maximum among the undefined five is 2. All nine defined tools are
used. There is no counter-example in either direction.

That is not a coincidence and it is not a merge Wombat should invent. It is the signature D12 describes —
*"one document written by several hands"* — with the committee's own instrument set visible underneath:
**the nine the committee defined once, deliberately, in an abbreviations table for the whole document.**

**This reshapes D12 and shrinks [T120].** The programme's recommendation was *"seed all fourteen verbatim
and ask the College to merge explicitly"*, on the ground that an invented merge is unauditable. That
principle stands — but the ask is now sharp and cheap instead of open-ended, and it comes with a proposed
mapping the College can accept or reject line by line:

| Undefined name | Where | Proposed reading | Strength |
|---|---|---|---|
| **Case note review** | PAED-006 | **CCA.** Its page-8 definition is literally *"review of clinical documentation and discussion of the reasoning and management plan recorded"* | Strong. This is D4's hoped-for alias, now evidenced |
| **Chart-stimulated recall** | PAED-002, PAED-012 | **CCA.** CSR is notes-plus-discussion-of-reasoning; the CCA definition describes CSR almost word for word | Strong |
| **Directly observed clinical examination** | PAED-007 | **Direct observation**, or a distinct OSCE-style exam | Weak — genuinely ambiguous, must be asked |
| **Learner feedback** | PAED-015 | An MSF template (already D35) | Moderate |
| **Portfolio and logbook review** | PAED-015 | Not a WBA instrument — a review of a body of work (already §5 item 3) | Strong |

If the College accepts the three strong readings, [T120] loses `chart_stimulated_recall_cpsa` and
`case_note_review_cpsa` from group 1 — **five plain seeds become three** (`cca_cpsa`, `rca_cpsa`,
`observed_clinical_exam_cpsa`) — and `portfolio_review_cpsa` leaves group 2, which was the group blocked on
file attachments (D34). **Do not implement any of this before the College answers.** The mapping is
recorded here so the ask can be made concretely; a merge Wombat makes silently is exactly what D12 refuses.

⚠ **D6 is untouched by all of this.** Page 8 says what each tool *is*; it never says which produce an
entrustment rung. The recommendation in the programme (the five Group-1 tools rated, reflective exercise /
clinical audit / portfolio review unrated) still needs College sign-off, and its trap still applies: a
directive with neither `minimum_level_field` nor `minimum_level_fixed` returns `NotGated()`
(`CreditApplier.cs:295-299`), i.e. `MinimumMet: true`.

## 5. NEW — Annexure B had never been extracted, and it carries phase 3's and phase 4's missing data

`T098-data/` held `annexure-a.json` and `epa-detail.json`. **Annexure B was never extracted.** It is now
`T098-data/annexure-b.json`, and it is fully self-consistent: the per-year column sums to **55**, the
per-semester column sums to **25**, and the decision-cadence column yields exactly the **21** decisions its
own prose claims (6 EPAs × 2 semesters + 9 annual). Every arithmetic claim checks out.

Three things in it are not derivable from Annexure A, which is what phase 3 and phase 4 have been planning
against:

**a. The per-semester quota is a published column, not a derivation.** Ten EPAs carry an explicit
per-semester figure; the five at one-per-annum carry an em-dash, and the prose says *"The remaining 5 are
scheduled throughout the year as opportunities arise."* So the rule is stated, not inferred: **an EPA above
one-per-annum splits evenly across the two semesters; an EPA at one-per-annum is not semester-bucketed at
all.** [T098] phase 3 (D18) should seed the published figure and seed `null` for the five, rather than
computing `RequiredCount / 2` and silently giving the five a quota of zero-point-five.

**b. The entrustment-decision cadence is independent of the observation cadence.** EPA 3 is observed **six
per annum (3 per semester)** and decided **annually**. Phase 4 has been treating "semester cadence" as one
concept; it is two, and they disagree on at least one EPA. The six semester-decided EPAs are **1, 2, 4, 5,
10, 12**; the other nine are annual.

**c. The exit rule is a sentence nothing in the product expresses.** *"By the end of training, a registrar
must be entrusted to supervise others (Level 5) in 9 EPAs and to practise unsupervised (Level 4) in the
remaining 6."* Wombat stores `MinimumLevelOrder` per item and has no notion of a programme-exit predicate
across the set. Not filed anywhere; noted here for phase 4.

## 6. 🚨 NEW — the source contradicts itself about MSF, and [T121]'s premise is one side of it

- **Annexure A lists MSF on all fifteen EPAs.** Verified by parse: `MSF` appears in the `tools` cell of
  15/15.
- **Annexure B's prose says** *"Multi-source feedback is specified in **eleven** of the fifteen EPAs."*

Both are in the same document. One is wrong.

[T121] opens *"MSF is required by all 15 EPAs and can credit none"* — that is Annexure A's side, taken as
fact. The defect it describes is real either way and its priority does not change; MSF is the most-required
tool in the catalogue under both readings. But **the arithmetic in D8 and D9 moves**: D9's *"15 × 8 = 120
returned questionnaires per registrar per year"* becomes 88 under Annexure B's count, and D8's *"two
semester campaigns supply 30 of the 55"* becomes 22. Neither changes the recommendation; both change the
number quoted to the College when asking.

Filed as **D37**. It is a one-line question and it should go in the same message as D1.

---

## Decisions this file closes or changes

| # | Was | Now |
|---|---|---|
| **D2** | Does page 8 define the instruments? | **CLOSED — no.** It defines nine tools in one sentence each, and separately the four information sources the EPAs actually cite. Form designs must still be commissioned |
| **D3** | What is CCA? | **CLOSED — Clinical Case Analysis.** The reconciling reading, as recommended; the expansion was different |
| **D5** | What does RCA stand for? | **CLOSED — Random Case Analysis.** Recommendation confirmed verbatim |
| **D4** | Is "Case note review" distinct from CCA? | **Still a College question, now evidenced.** Page 8 defines CCA as documentation review + reasoning discussion, and never defines "Case note review". Alias is the strong reading |
| **D12** | Seed fourteen verbatim, or collapse? | **Reshaped.** The nine/five partition is clean and asymmetric. Still ask the College — but with a proposed mapping, and with three of the five strongly evidenced |
| **D6** | Which tools produce a rung? | **Unchanged.** Page 8 is silent. Still needs the College |
| **D37** | *(new)* Is MSF on eleven EPAs or fifteen? Annexure A and Annexure B disagree | Ask with D1. Changes the numbers in D8 and D9, not their recommendations |
| **D38** | *(new)* Page 4 requires a CCC decision to be grounded in the standard information sources, *"never from a single form"*. Wombat records entrustment decisions with no stated evidence basis. Is that a phase-4 requirement? | **Recommendation: yes, and it belongs in phase 4's task file** — which does not exist yet (§5 item 1) |

## Related

Executes Wave 1 item 2 of `Programme/EPA-PROGRAMME.md`. Feeds [T120] (which shrinks), [T121] (whose premise
gains a caveat), [T098] phase 3 (which gains a published per-semester column) and phase 4 (which gains a
decision cadence, an exit rule and D38). Source: `EPA version 11.1.docx`, pages 4 and 8, Annexure A,
Annexure B.
