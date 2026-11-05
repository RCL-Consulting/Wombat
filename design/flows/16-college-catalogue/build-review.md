# Flow 16 build: the review

The build on `t394` (head `770c0b27`: § 0 `9f25092c`, lanes A–D, the sweep fix `0e6ff53a`, lane W `2e29f5df`) reviewed
2026-11-05 from two sides, each a Sonnet agent: **the code** (`build-review-code.md`, `179d8bfc`: 0 must, 4 should, 8 nit;
83 Application tests on the new handlers run, green; the PostgreSQL suites not run by it) and **the pages and the runbook**
(`build-review-pages.md`, `60b06f21`: 1 must, 5 should, 11 nit; 20 groups of words on no board, about 38 strings; a static
read). The two files are kept as they are; this one merges them. The must-fix was checked by the integrator against the
runbook before it was carried.

## What holds

- No cross-College or cross-institution read or write; no exception text on a page; no refusal that stages a change
  before it throws. Q18's order, T395's reorder guard, Q19's lock, the College and speciality name races, Q10's holder
  and the reach read (by `InstitutionId` + `CurriculumId` + `IsActive`, never `AdoptionId`) hold; D57 is read-only.
- The runbook's quoted product words in the changed steps match the build character for character, but for M1. The
  reach counts at 6.14, 6.17, 6.28, 6.29 and the post-act-6 Hold back row hold.
- Five of the lanes' six departures from the boards are right; the sixth is S2 below.
- The suites on `t394`: 836/4402/1035/52/4670/524. The sweep: 0 fails after Curricula moved to the 1100 px stack.

## Must-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | pages M1 | A.7.10 expects "1 registrar in training on it" on 11.2. Dr Ndlovu is the only registrar on 11.2 (6.36), and his erasure (A.1.12) ends his programme before A.7.10, so the reach reads none. The build is right; the runbook is wrong. | `appendix-cross-cutting.md:47` ("nothing after it needs Dr Ndlovu"), `:381-396`, `:1750-1756`; `act-6-catalogue.md:1052` | The appendix's two lines and `states.md:858` read the build's none sentence. |

## Should-fix

1. **(pages S1) Q10's refusal moves.** After the refusal the item page reads its picker again, so the refusal sits
   under another EPA or leaves the field for the empty state; `states.md:852` cannot happen as written. Keep the refused
   EPA chosen with the refusal under it until the user changes it, as R3-F14 draws.
2. **(pages S2) Cancel on Save's question** returns focus to Deactivate… on the College, speciality, sub-speciality and
   EPA forms; it must return to Save, as `CurriculumEdit` does.
3. **(pages S3) DESIGN.md** does not record flow 16's patterns (the tree, the record pages, the rung refusal, Q19's
   read-out, the reach line). Add them, with the banner listing flow 16.
4. **(pages S4) Unavailable** is drawn in two markups across the new pages; one.
5. **(pages S5) The Status checkbox** has two patterns, and on the College and EPA forms its name reads "Status Active";
   one pattern, named by its label.
6. **(code S-3) No test** shows the scale update or delete refusing a CollegeAdmin or an InstitutionalAdmin; add both.
7. **(code S-4) Q18's order** is proved for two of its four steps; prove the other two.
8. **(code S-2) The new reads** (the tree, the reach, "in use") are proved only on the in-memory provider; add a
   PostgreSQL test each (`Wombat.Integration.Tests`).
9. **(code S-1) The Q16 and Q19 guards are check-then-save** with no lock or recheck, so an adoption or a pin committed
   between the check and the save slips past. The question for the operator below.

Nits: code eight (a split doc block in `EntrustmentScaleReferences.cs`, two readers of a unique violation, the College
name pre-check's case-sensitive collation, the delete's holder links chosen by comparing sentences, "in use" scanning
every ended record on each scale page load, `SubSpecialityFixed` computed for every caller, a scale's name race checked
only before, `UpdateSpeciality` without a College-exists read and one unused `@using`); pages eleven
(`build-review-pages.md`). The fix pass takes them as one list.

**Words on no board:** about 38 strings in 20 groups (`build-review-pages.md` § Words), for the operator to accept or
reword; flow 15's operator accepted the lanes' new words as a list.

## For the operator

- **F1. The fix pass's scope.** *Recommendation:* M1, should-fixes 1–8 and the nits, in one Opus pass; S-1 (9) filed as
  its own task, since a lock on adoption and pinning reaches flows 15 and 17 and the credit locks' pattern
  (`IEpaCreditLock`, `ITraineeCreditLock`), and nothing is live.
- **F2. The words on no board.** *Recommendation:* accept them as listed, the fix pass rewording any the pages review
  marks wrong.
</content>
</invoke>
