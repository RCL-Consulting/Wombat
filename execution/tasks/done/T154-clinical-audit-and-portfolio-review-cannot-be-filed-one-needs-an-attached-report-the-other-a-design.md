---
id: T154
title: Clinical audit and portfolio review cannot be filed: one needs an attached report, the other a design
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T154 — Clinical audit and portfolio review cannot be filed: one needs an attached report, the other a design


> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Neither unlocks an EPA (every EPA already permits a seeded tool); both are unrated evidence that
credits nothing (D6, D7). What is missing is fidelity to the College's tool mix.
**Surfaced:** 2026-09-24, split out of [T120] on the operator's call ("Defer both").

## Symptom

Annexure A names **Clinical audit** on EPAs 1, 2, 3 and **Portfolio and logbook review** on EPA 15. Neither can be
filed: no seed exists. The vocabulary keys (`clinical_audit`, `portfolio_review`) exist and the allow-lists name them
(T122), so the day a seed lands it is already permitted on the right EPAs.

## Why each is blocked

- **Clinical audit** (page 8: *"a cycle of measurement against an agreed standard, followed by change and
  re-measurement"*) wants its report attached, and Wombat has no file storage: `FieldType.File` renders a placeholder.
  An attachment is personal data — subject-access export (`AccessReportBuilder`), erasure (`ErasureExecutor`), the
  portfolio PDF and the CSP all have to handle it. D34 recommends a text field holding a link as the interim.
- **Portfolio and logbook review** (EPA 15: *"teaching sessions, journal club presentations, feedback received, and
  reflective entries"*) reviews a body of work Wombat already holds. A form that asks the reviewer to re-type it is the
  wrong shape; it wants a review that names the trainee and a period and points at the existing portfolio export.

## What to build

Decide D34 (attachments or a link field) and the portfolio review's shape, then seed each on T120's checklist:
unrated, `"counts_for": []`, no `rated_level_field`, a `WbaToolKey`, honest `required` flags and declared
`validation` per transition (T105), a user field for the reviewer if one signs it off (T102).

## Verification

- [x] Each seed is registered in `ActivityTypeSeedCatalogue` with its `WbaToolKey`, created on a fresh database, and a
  second boot republishes nothing.
- [x] Filing each against an EPA whose list names it works; against one that does not, the EPA is not offered.
- [x] Neither credits anything, and neither appears on an entrustment trajectory.

## Related

[T120] (the checklist), D6, D7, D34, T106 item 8 (file fields render a placeholder), [T122] (the allow-lists).

---

## As built — 2026-09-24

**Decisions**
- **D34 is closed: the audit's report is a link**, a text field accepting only `http(s)` URLs.
- **The portfolio review is a review record**, not a re-typed portfolio:
  - The trainee names the period, the reviewer and, optionally, the export file.
  - The Assessor records `reviewed_on`, the evidence reviewed and comments, then signs off or returns it.
  - It is dated by `period_to`, so it falls in that period's committee window (D27).
  - The reviewer is an Assessor, not a CommitteeMember: a committee member producing the evidence the committee
    judges would be a conflict.
- **D45, from the review:** an instrument that credits nothing is gated by the EPA it is evidence for, at the same D20
  moments. The operator may overrule; see EPA-PROGRAMME § 3D and College question 12 (the audit on EPA 10).

**Seeds**
- `clinical_audit_cpsa`: PAED-001 to 003.
- `portfolio_review_cpsa`: PAED-015.
- Both are unrated, credit nothing, and take the `WbaToolKey` of their instrument. Each has `validation` on every
  transition, `evidence_epa_field`, and a user nominee field of role Assessor.
- Trainees have an Export Portfolio nav link.

**Evidence**
- Suites on master: Domain 436, Application 1511, Infrastructure 761, Architecture 31, Web 589, Integration 45.
- Browser on dev, scripted Chrome:
  - **Boots.** The first boot inserted types 20 and 21 at version 1; the second republished nothing ("0 republished,
    21 unchanged").
  - **Pickers.** The audit offers PAED-001 to 003, the review PAED-015, and the reflective exercise PAED-001, 003,
    008 and 014.
  - **Audit, activity 33.** "On the drive" was refused ("Audit report (link): Value does not match the required
    format."). A valid link submitted, and the Assessor signed it off (`signed_off`). No credit: every
    `CreditedItemCount` is NULL and the progress page is byte-identical.
  - **Review, activity 34.** A future `period_to` was refused. For the period 2026-01-01 to 2026-06-30, the evidence
    group rendered as a named fieldset. It was signed off with `ObservedOn` 2026-06-30, and there was no credit.
  - **Forged EPAs.** A forged PAED-010 audit was refused: "EPA: Clinical audit cannot be used as evidence for
    PAED-010 … The curriculum accepts Direct observation or MSF for this EPA." A reflective exercise on PAED-002 and a
    portfolio review on PAED-001 were refused likewise. No row was written.

**Seen, left for other tasks**
- The evidence options show raw keys ([T191]).
- The future-period refusal says "encounter date" for a review period ([T197], the wording task).

