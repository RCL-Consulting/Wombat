# Literature-backed critique of the EPA Book's prescriptions

Companion to `EPA Book/evaluation.md`. The evaluation treats the book as authoritative and scores Wombat against it. This document catalogues peer-reviewed challenges to specific book prescriptions, so that the Wombat plan can make informed choices about where to follow the book and where to deviate.

**Principle:** the book (Ten Cate et al.) remains the primary reference. These citations supplement, not replace, it. Wombat aims to be a *good* WBA platform, which means occasionally extending or qualifying the book — always with hard reference.

**Recall caveat:** these citations were produced from recall. Author names and journals should be correct; years, volumes, and page numbers are ≥95% confidence but not verified. The intent is that a reviewer verifies each against PubMed before quoting externally. Items flagged *verify* have lower confidence.

---

## 1. The 5-level supervision scale is not a single construct

**Book position:** Chapter 19 — the standard five-level entrustment-supervision scale (observe → direct → indirect → unsupervised → supervise others) is the canonical anchor.

**Contrary / complicating evidence:**

- Crossley J, Johnson G, Booth J, Wade W. *Good questions, good answers: construct alignment improves the performance of workplace-based assessment scales.* Med Educ. 2011;45(6):560–9.
  - Shows scale performance improves substantially when anchors map to *authentic supervision decisions* rather than abstract Likert descriptors.
- Rekman J, Gofton W, Dudek N, Gofton T, Hamstra SJ. *Entrustability scales: outlining their usefulness for competency-based clinical assessment.* Acad Med. 2016;91(2):186–90.
  - Reviews multiple entrustability scales and documents substantial construct heterogeneity between them.

**Implication for Wombat:**
- Do not assume the seeded five-level scale is universally canonical.
- Keep anchors editable per institution (already true via `EntrustmentScale` aggregate).
- Document the Crossley construct-alignment argument in admin guidance for anyone creating a custom scale.

---

## 2. Rater leniency and ceiling effects are systemic in direct observation

**Book position:** Chapters 17, 20 — acknowledges rater leniency but treats it as solvable through faculty-development and calibration training.

**Contrary / complicating evidence:**

- Kogan JR, Holmboe ES, Hauer KE. *Tools for direct observation and assessment of clinical skills of medical trainees: a systematic review.* JAMA. 2009;302(12):1316–26.
  - Systematic review documenting ceiling effects in mini-CEX and DOPS across multiple studies.
- Pelgrim EA, Kramer AW, Mokkink HG, van den Elsen L, Grol RP, van der Vleuten CP. *In-training assessment using direct observation of single-patient encounters: a literature review.* Adv Health Sci Educ. 2011;16(1):131–42.
- Williams RG, Klamen DA, McGaghie WC. *Cognitive, social and environmental sources of bias in clinical performance ratings.* Teach Learn Med. 2003;15(4):270–92.

**Implication for Wombat:**
- Before investing heavily in rating-based analytics (Phase 3 trajectory charts, T035), build rating-distribution diagnostics (T038).
- Distribution diagnostics will reveal whether the rating data supports the intended interpretation; analytics built on ceilinged data mislead.

---

## 3. Supervisor and assessor are structurally conflicting roles

**Book position:** Treats supervisor assessments as the primary data source without surfacing the role conflict inherent in asking a supervisor to both support and rigorously rate the same trainee.

**Contrary / complicating evidence:**

- Govaerts MJ, van der Vleuten CP, Schuwirth LW, Muijtjens AM. *Broadening perspectives on clinical performance assessment: rethinking the nature of in-training assessment.* Adv Health Sci Educ. 2007;12(2):239–60.
  - Argues workplace assessors operate under competing agendas (supervision, relationship, learning support) that systematically colour their judgements.
- Govaerts MJ, Schuwirth LW, van der Vleuten CP, Muijtjens AM. *Workplace-based assessment: effects of rater expertise.* Adv Health Sci Educ. 2011;16(2):151–65.

**Implication for Wombat:**
- This is a data-quality issue, not a nice-to-have. Elevate Phase 4 (faculty development) above Phase 3 (analytics) in the current `book-fidelity-plan.md` sequencing.
- Consider moving T036 (assessor training status) and T038 (bias diagnostics) ahead of T035 (trajectory charts).

---

## 4. Feedback reception is relational, not informational

**Book position:** Chapter 19 — treats feedback as content to be delivered clearly and captured in narrative fields.

**Contrary / complicating evidence:**

- Sargeant J, Mann K, Sinclair D, van der Vleuten C, Metsemakers J. *Understanding the influence of emotions and reflection upon multi-source feedback acceptance and use.* Adv Health Sci Educ Theory Pract. 2008;13(3):275–88.
- Sargeant J, Lockyer J, Mann K, et al. *Facilitated reflective performance feedback: developing an evidence- and theory-based model that builds relationship, explores reactions and content, and coaches for performance change (R2C2).* Acad Med. 2015;90(12):1698–706.
  - The R2C2 model (Relationship, Reaction, Content, Coaching) explicitly foregrounds relationship and emotional reaction before content delivery.
- Eva KW, Armson H, Holmboe E, Lockyer J, Loney E, Mann K, et al. *Factors influencing responsiveness to feedback: on the interplay between fear, confidence, and reasoning processes.* Adv Health Sci Educ. 2012;17(1):15–26.

**Implication for Wombat:**
- Structure the activity-completed → trainee workflow around R2C2 phases, not just "assessor writes, trainee reads".
- Add a trainee-response field and a "coaching conversation" state to the seeded observation workflows (mini-CEX, DOPS, CbD, ACAT). This is a schema change on the workflow side, not a new aggregate.
- Suggested new task: **T046 — R2C2-aligned feedback workflow on seeded observation types.** Slot into Phase 1 or early Phase 3.

---

## 5. Validity after Messick/Kane is contested, not settled

**Book position:** Chapter 5 presents Messick and Kane validity as the governing framework for workplace-based assessment.

**Contrary / complicating evidence:**

- Hodges B. *Assessment in the post-psychometric era: learning to love the subjective and collective.* Med Teach. 2013;35(7):564–8.
  - Argues formal psychometric validity has been stretched beyond its design intent when applied to aggregated heterogeneous workplace evidence.
- Cook DA, Brydges R, Ginsburg S, Hatala R. *A contemporary approach to validity arguments: a practical guide to Kane's framework.* Med Educ. 2015;49(6):560–75.
  - A more sympathetic treatment — but still careful about what Kane can and cannot warrant for workplace aggregations.
- Schuwirth LW, van der Vleuten CP. *A history of assessment in medical education.* Adv Health Sci Educ. 2020;25(5):1045–56.

**Implication for Wombat:**
- Do not over-promise defensibility in UI copy or admin documentation.
- The platform captures evidence; whether the *interpretation* by a committee is valid remains an institution-level judgement.
- In portfolio PDF export (T023 artefact) prefer "evidence record" framing over "validated assessment score".

---

## 6. Sampling thresholds are not empirically fixed

**Book position:** Chapter 17 cites specific thresholds (e.g., "≥ 8 direct observations" for undergraduate EPAs).

**Contrary / complicating evidence:**

- Moonen-van Loon JM, Overeem K, Donkers HH, van der Vleuten CP, Driessen EW. *Composite reliability of a workplace-based assessment toolbox for postgraduate medical education.* Adv Health Sci Educ. 2013;18(5):1087–102.
  - Generalisability-theory analyses showing reliability coefficients depend on instrument mix, rater pool, and context — no universal threshold holds.

**Implication for Wombat:**
- Keep `CurriculumItem.RequiredCount` configurable per institution and per EPA (already the case).
- Do not ship the book's consensus numbers as hard defaults. Surface uncertainty in admin documentation.
- Phase 3 sampling-adequacy views (T034) should flag *diversity* of sources and raters, not just absolute counts.

---

## 7. Patient outcomes are under-used as an evidence source

**Book position:** Touches RAMP (resident-attributed metrics, product evaluation) in Chapter 17 but does not advocate strongly for it.

**Contrary / complicating evidence:**

- Asch DA, Nicholson S, Srinivas S, Herrin J, Epstein AJ. *Evaluating obstetrical residency programs using patient outcomes.* JAMA. 2009;302(12):1277–83.
- Bansal N, Simmons KD, Epstein AJ, Morris JB, Kelz RR. *Using patient outcomes to evaluate general surgery residency program performance: a proof of concept.* JAMA Surg. 2016;151(2):111–9.
- Sirovich BE, Lipner RS, Johnston M, Holmboe ES. *The association between residency training and internists' ability to practice conservatively.* JAMA Intern Med. 2014;174(10):1640–8.

**Implication for Wombat:**
- Add a Phase 5 task for outcome-registry ingestion: **T047 — Patient-outcome data source for EPA evidence.**
- Not urgent, but a genuine differentiator. Most mature WBA tools lack this, and the literature supports going beyond the book here.
- Scope: a generic external-data-source connector per institution, mapped into `Activity`-shaped evidence records of a seeded `outcome_metric` type.

---

## 8. Assessment burden on trainees is a documented concern

**Book position:** Chapter 20 addresses challenges but does not centre trainee burden as a design constraint.

**Contrary / complicating evidence:**

- Boyd VA, Whitehead CR, Thille P, Ginsburg S, Brydges R, Kuper A. *Competency-based medical education: the discourse of infallibility.* Med Educ. 2018;52(1):45–57.
  - Critical discourse analysis of assumptions embedded in CBME advocacy.
- Van Melle E, Frank JR, Holmboe ES, Dagnone D, Stockley D, Sherbino J, et al. *A core components framework for evaluating implementation of competency-based medical education programs.* Acad Med. 2019;94(7):1002–9.
  - Sympathetic but honest about CBME implementation failure modes.
- LaDonna KA, Ginsburg S, Watling C. *"Rising to the Level of Your Incompetence": What Physicians' Self-Assessment of Their Performance Reveals About the Imposter Syndrome in Medicine.* Acad Med. 2018;93(5):763–8.
- Watling CJ, Ginsburg S. *Assessment, feedback and the alchemy of learning.* Med Educ. 2019;53(1):76–85.

**Implication for Wombat:**
- Keep the trainee-facing surface area deliberately small and non-performative.
- A RICH capture (T029) should be opt-in per institution, not default.
- Avoid gamification, public leaderboards, and performative progress indicators on trainee dashboards.
- Reinforces the argument for T042 (self-assessment pairing) being handled carefully — it can either support or aggravate the imposter-syndrome dynamic depending on framing.

---

## Critiques considered but not citable from recall

The following points were raised in discussion but dropped from this document because I could not produce a hard reference. They may be valid — the absence of citation here reflects my recall limits, not necessarily the absence of literature.

- **"Four sources taxonomy is awkward."** No named critique paper; the taxonomy is primarily a Ten Cate framing. Omitted.
- **"Goodhart's law in WBA."** Campbell DT, *Assessing the impact of planned social change* (1976) is the originating reference for the general principle, but I cannot cite a specific WBA-applied paper. If this is load-bearing for a plan decision, the user should search for recent work on "gaming of competency-based assessment".
- **"Cross-cultural EPA transplant."** Implementation work exists (Frank JR, Snell L, Holmboe ES on CanMEDS and CBME roll-out internationally) but I do not have a sharp South-African or developing-world critique citation. *verify*.
- **"Narrative feedback-as-textarea is theatre."** Editorial framing, not a citable claim. The Sargeant/Eva work in §4 supports a careful version of it.

---

## Proposed plan adjustments supported by the above

Drawn from the implications sections, in priority order:

1. **Re-sequence Phase 4 ahead of Phase 3** in `Rewrite/book-fidelity-plan.md`. Rationale: §3 (supervisor-assessor conflict) and §2 (rater leniency) mean analytics built on uncalibrated assessor data will mislead. Faculty development is a data-quality prerequisite, not a polish item.
2. **Add T046 — R2C2-aligned feedback workflow** (§4). Schema-level extension of seeded observation workflows to include trainee response and coaching conversation states. Estimated effort: M.
3. **Add T047 — Patient-outcome data source** (§7). Phase 5 item. Generic connector + seeded `outcome_metric` activity type. Estimated effort: L.
4. **Soften T028 and T029 ambitions** (§5, §8). Prospective/retrospective pair and A RICH rubric stay in scope but are opt-in, not default-on. Adjust acceptance criteria in the task files when written.
5. **Update `EPA Book/evaluation.md` scoring rationale** to note that some low scores (§6 trust & validity, §7 faculty development) reflect areas where the book's prescription is itself contested, not just areas of Wombat shortfall.

---

## Status

This document is **reference material**, not a roadmap. The forward plan lives at `Rewrite/practical-plan.md` and is pragmatism-driven. The citations here serve one purpose: when an external reviewer asks "why doesn't Wombat do X?", the plan's "Known compromises" section points back to the relevant row above.

User will verify citations against PubMed / DOI before any are quoted externally. Flag any that do not resolve; they will be corrected or withdrawn.
