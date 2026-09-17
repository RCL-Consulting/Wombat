# T108 — A trainee can record an assessment against an EPA their institution has not adopted, and it is silently uncredited

**Status:** open
**Surfaced:** 2026-09-17, browser-verifying T070 end to end.
**Severity:** High — the assessment completes, the data is stored, the assessor is thanked, and **nothing
counts**. There is no error, no warning, and nothing on any screen says so.

## What happened

Verifying T070, a paediatric trainee created a CPSA Mini-CEX against `PAED-001 — Providing paediatric
emergency care to children` (EPA id 17), the assessor rated it at the top rung and completed it. The
activity reached `completed`, `DataJson` holds the assessor's rating and feedback — and
`CurriculumItemProgresses` has **no row**.

```
TraineeProfile(dlamini).CurriculumId = 2   (FCPaed(SA) Part 1, v2026.1)   AdoptionId = 1
Epa 17 (CPSA PAED-001) -> CurriculumItem 17 -> CurriculumId = 3   (Paediatric EPA Curriculum v11.1)
InstitutionCurriculumAdoptions: exactly one row — institution 2 -> curriculum 2
```

`CreditApplier` credits only the adopted curriculum version (T091, by design and correctly). The EPA the
trainee picked is not in their curriculum, so no item matched and no credit was applied.

## Root cause — two gates, and only one was opened

**Speciality scope** and **curriculum adoption** are independent:

- T099 gave the paediatric cohort a Speciality-3 scope so the CPSA tools and CPSA EPAs became *visible and
  selectable*. That worked.
- Nobody adopted the CPSA curriculum for KGK, and no trainee is pinned to it. So everything selectable from
  the new catalogue is **uncreditable for these trainees**.

The EPA picker now offers **30 EPAs** — both catalogues interleaved, `PAED-001` through `PAED-015` twice
with different titles and no visual distinction. Fifteen of them silently produce no credit. A registrar
has no way to tell which fifteen.

**This is a consequence of T099.** Before it, the new catalogue was invisible, which was a different and
more honest failure. Making it visible without adoption is worse: it looks like it works.

## Fix — the picker should not offer what cannot be credited

1. **Filter the EPA options to the subject's adopted curriculum.** `ActivityReferenceDataService` scopes
   EPA options by institution/speciality; it should additionally scope by the subject's active adoption
   when the activity has a subject. This is the real fix and it is not specific to paediatrics — any
   institution mid-adoption has the same hole.
2. **Or warn at submit** when the chosen EPA resolves to no curriculum item for the subject. Weaker: it
   still lets the encounter be recorded uncredited, but at least the trainee knows.
3. **Or adopt the CPSA curriculum and re-pin the cohort** — that is T104, and it carries the 5-rung/6-rung
   re-point hazard, so it is not a quick unblock.

Recommendation: **1**, as a defect in its own right. Then T104 on its own merits.

## Wider point worth keeping

`CreditApplier` returning "no matching curriculum item" is indistinguishable, from every surface, from
"credit applied". Whatever else is decided, a completed activity that credited **nothing** should be
visible somewhere — on the activity, in the audit entry, or on the trainee's progress page. Silent
non-credit is how a registrar reaches the end of a year believing 55 encounters were logged.

## Verification

- A trainee whose institution has adopted only curriculum 2 is not offered curriculum 3's EPAs.
- An encounter that credits nothing says so, somewhere a human will see it.
- After adoption (T104), the same flow produces a `CurriculumItemProgress` row.

## Evidence

Dev database, activity id 11, created and completed through the browser on 2026-09-17. Left in place as
evidence; `CurriculumItemProgresses` has no row referencing it.

## Related

Consequence of T099. Blocks meaningful use of the v11.1 catalogue as much as T070 did. Interacts with T104
(adoption + rating remap) and T098 phase 3 (per-year quota, which counts the rows this never creates).
