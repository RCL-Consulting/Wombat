# Wombat vs. the EPA Book — evaluation

Scored against 92 requirements extracted from chapters 1–24 of *Entrustable Professional Activities in Medical Education* (the canonical reference in `EPA Book/chapters/`). Produced 2026-04-20 against the post-T016 codebase.

> **Context:** this evaluation captures the gap against the textbook's *complete* prescription. It is not a todo list. The forward plan is `Rewrite/practical-plan.md`, which selectively addresses the gaps that matter for a working hospital training programme. Several low scores below (§6 trust & validity, §7 faculty development) reflect areas where the book's prescription is contested in the literature (see `EPA Book/critique.md`) and the practical plan deliberately does not chase them.

**Legend:** ✅ full · 🟡 partial · ❌ gap.

---

## 1. EPA structure & content

| # | Requirement | Status | Note |
|---|---|---|---|
| 1 | 8-component EPA specification (risks, KSA, sources, levels, expiry, framework links) | 🟡 | `Epa` has title/description/context/KSA; no fields for risks, required sources, expiration, or framework mapping |
| 5 | Nested EPAs (small → broad progression) | ❌ | Not modeled |
| 6 | Core vs. elective EPA distinction | ❌ | No flag on `Epa` |
| 7 | Trans-disciplinary EPAs with context restrictions | ❌ | EPA is pinned to one SubSpeciality |
| 9 | EQual-style quality rubric for EPA definitions | ❌ | No rubric tooling |
| 10 | EPA ↔ competency-framework matrix | ❌ | No competency-domain aggregate |
| 85 | STAR expiration tracking | ❌ | STARs not issued as formal artefacts (see §2) |

## 2. Entrustment / supervision scales

| # | Requirement | Status | Note |
|---|---|---|---|
| 12 | 5-level standard supervision scale | ✅ | Seeded O-R scale |
| 13 | Configurable scale per EPA/context | 🟡 | `EntrustmentScale` supports custom scales, but assignment is per `AssessmentForm`, not per EPA |
| 14 | Separate retrospective (ES provided) vs. prospective (ES recommended) ratings | ❌ | One scale field per assessment; the prospective/retrospective distinction is not captured |
| 16 | **Ad-hoc entrustment vs. summative entrustment decision** | ❌ | `CommitteeDecision` exists but is ARCP-shaped (Pass/Conditions/Fail), not *per-EPA summative entrustment*. There is no per-EPA "authorised at level X" decision object. |
| 18 | **STAR (Statement of Awarded Responsibility) generation** | ❌ | **Naming collision worth flagging:** Wombat's seeded `star_reflection` is a Situation-Task-Action-Result *reflection*, **not** Ten Cate's STAR. The formal authorisation artefact is absent. |
| 83 | Entrustment decisions trigger real workflow change (scheduling/supervision) | ❌ | Decisions are recorded but do not propagate to downstream supervision state |

## 3. WBA instruments & four sources

| # | Requirement | Status | Note |
|---|---|---|---|
| 20 | Four sources: direct obs, conversation, longitudinal, product | ✅ | Mini-CEX/DOPS, CbD/ACAT, MSF, Procedure Log / Research / QI / Teaching |
| 23 | MSF with anonymised aggregation | ✅ | Hard-coded `MsfCampaign` with hashed respondents; category averages |
| 25 | Knowledge/skills exam integration | ❌ | No exam ingestion |
| 28 | Configurable sampling thresholds for high-stakes decisions | 🟡 | `CurriculumItem.RequiredCount` + `MinimumLevel` + `WindowMonths` cover count/level/time, but not source/context diversity |
| 30 | Sampling-bias tracking (trainee-initiated vs. supervisor-initiated) | 🟡 | Data present (`CreatedByUserId`, actor rules) but not surfaced or analysed |

## 4. Committee / programmatic assessment

| # | Requirement | Status | Note |
|---|---|---|---|
| 31, 37 | Committee aggregate with diverse membership | ✅ | `DecisionPanel` + `PanelMember` |
| 32 | Summative decisions by committee, not individual | ✅ | `CommitteeDecision` workflow |
| 33–34 | Dashboards aggregating all sources, trajectory, cohort comparison, sampling adequacy | 🟡 | Per-CurriculumItem progress exists; **no cohort comparison, no trajectory charts, no sampling-by-source view** |
| 38 | Structured consensus procedure + bias mitigation | ❌ | Decision is recorded as single rationale; no capture of dissent, rotating speakers, quorum rules |
| 39 | Dissent/justification logging | 🟡 | Rationale + conditions stored; no separate dissenting-opinion field |
| 40 | Identify dyscompetence *and* excellence | ❌ | Decision categories are ARCP-style (Pass/Conditions/Fail/Appeal); no "excellence" flag |
| 41 | Interim formative CCC feedback without high-stakes decision | ❌ | `CommitteeReview` is terminal Decided→Ratified; no formative-only mode |
| 43 | Promotion-in-Place (time-variable advancement) | ❌ | Not modeled |

## 5. Curriculum & progress

| # | Requirement | Status | Note |
|---|---|---|---|
| 44–45 | Individualised, rotation-aware EPA pathways | 🟡 | Curriculum assigned per trainee; no rotation/placement entity driving "which EPAs this month" |
| 47 | Time-variable graduation (not calendar-based) | 🟡 | `CurriculumItemProgress` supports evidence-based readiness; no competency-based graduation gate wired to progression |
| 48 | Cohort trends (% reaching level at 12/24/36 mo) | ❌ | No cohort analytics |
| 49 | Configurable supervision-level expectations per training stage | ❌ | `MinimumLevel` is global per `CurriculumItem`, not stage-indexed |
| 51 | Formal transitions between training phases | ❌ | Not modeled |

## 6. Trust & validity

| # | Requirement | Status | Note |
|---|---|---|---|
| 52 | A RICH (Agency, Reliability, Integrity, Capability, Humility) in assessments | ❌ | No seeded fields for trustworthiness attributes |
| 57 | Messick/Kane validity-evidence documentation | ❌ | No validity metadata aggregate |
| 59 | Shared subjectivity / multiple-rater triangulation | 🟡 | Multi-rater is implicit via multiple activities; no explicit triangulation view |
| 60 | Holistic interpretation — avoid raw averages | 🟡 | Progress view shows count + min-level count; MSF view shows averages (which the book warns against for high-stakes use) |
| 61 | Growth-curve reliability (trajectory, not test-retest) | ❌ | No time-series |
| 66 | Assessor bias/extremity flags | ❌ | Data present, analytics absent |

## 7. Faculty development

| # | Requirement | Status | Note |
|---|---|---|---|
| 63 | Assessor-training status tracking | ❌ | No training-completion field on `AssessorProfile` |
| 64–65 | Frame-of-reference / calibration tooling | ❌ | Inventory explicitly flagged this gap |
| 66–67 | Bias monitoring & nudges | ❌ | Not implemented |
| 68 | Accessible feedback-skill resources | ❌ | No in-app training/LMS |

## 8. Trainee-facing

| # | Requirement | Status | Note |
|---|---|---|---|
| 69 | Current authorised supervision level per EPA | 🟡 | Shows progress toward required count/level; doesn't surface a discrete "you are authorised at level N" status |
| 71 | Trainee access to own data | ✅ | `MyActivities`, portfolio PDF |
| 72 | Self-assessment compared to supervisor assessment | ❌ | No self-assessment pairing |
| 73 | Trainee-initiated observation request | ✅ | Assessment-request workflow supports this |
| 75 | Reflect on feedback, document learning goals | 🟡 | Trainee can create a new STAR reflection; no structured "response to feedback" thread |
| 78 | "Stuck on EPA" flag | ❌ | No stagnation detection |

## 9. Governance & defensibility

| # | Requirement | Status | Note |
|---|---|---|---|
| 80 | Immutable audit | ✅ | `AuditEntry` append-only, MediatR pipeline |
| 81 | Role-scoped access | ✅ | Nine roles + claim-based scoping |
| 84 | Signed STAR document | ❌ | See §2 gap |
| 87 | Grade-inflation / CCC-norm detection | ❌ | No benchmarking |
| 91 | Accreditor-specific export formats (ACGME/ABMS/HPCSA) | 🟡 | Portfolio PDF is generic; no accreditor-shaped exports |
| 92 | Decision defensibility trail (what data, who, what alternatives) | 🟡 | "What data" and "who" captured; "what alternatives considered" not prompted |

---

## Summary

**Strengths — genuinely book-aligned.** Wombat's schema-driven activity platform cleanly covers the "four sources of evidence" (ch. 17) and lets programmes add new WBA instruments without code. MSF anonymity, append-only audit, role scoping, and committee-with-appeal workflow align well with programmatic-assessment governance (ch. 6, 21).

**Theme-level scores.**

| Theme | Score |
|---|---|
| 1. EPA structure & content | 🟡 3/10 |
| 2. Entrustment scales | 🟡 4/10 |
| 3. WBA instruments & four sources | ✅ 8/10 |
| 4. Committees / programmatic assessment | 🟡 5/10 |
| 5. Curriculum & progress | 🟡 4/10 |
| 6. Trust & validity | ❌ 2/10 |
| 7. Faculty development | ❌ 1/10 |
| 8. Trainee-facing | 🟡 6/10 |
| 9. Governance & defensibility | ✅ 7/10 |

### Top five load-bearing gaps

These would most change the platform's fidelity to the book:

1. **STAR / per-EPA summative entrustment decisions** (ch. 7, 10, 18). The formal *"authorised for EPA X at level Y"* artefact does not exist. The seeded `star_reflection` activity type reuses the STAR acronym for a reflection format, which is a book-vs-product terminology collision worth resolving.
2. **Prospective vs. retrospective entrustment ratings** on each observation (ch. 19). Today, an assessment captures one rating; the book treats these as semantically distinct.
3. **Longitudinal / cohort analytics** (ch. 6, 21). Committee dashboards lack trajectory charts, cohort percentiles, sampling-adequacy-by-source views, and grade-inflation checks. The biggest functional gap between the codebase and the book's picture of programmatic assessment.
4. **A RICH trustworthiness attributes** (ch. 4, 10). Narrative feedback is free-form; no structured capture of Agency, Reliability, Integrity, Capability, Humility — which the book treats as the foundation of grounded trust.
5. **Faculty-development layer** (ch. 17, 19, 23). No assessor-training state, no calibration/frame-of-reference tooling, no bias flags on rating patterns. Chapter 23 treats this as non-optional for a defensible system.

### Lower-priority but worth logging

- Nested EPAs (ch. 10)
- Core-vs-elective flag (ch. 10)
- EQual rubric tool for EPA quality audit (ch. 11)
- Self-vs-supervisor assessment comparison (ch. 4)
- Stage-indexed supervision-level expectations (ch. 1)
- Time-variable promotion-in-place (ch. 18)
- Formal phase-transition readiness (ch. 16)

### Scope note

Findings are scoped to what the book prescribes; many of the gaps are design choices a programme director might legitimately defer. The T019-b…g follow-ups listed in `Rewrite/current_state.md` do not cover any of the top-five gaps above — they are builder-UX, not book-fidelity.
