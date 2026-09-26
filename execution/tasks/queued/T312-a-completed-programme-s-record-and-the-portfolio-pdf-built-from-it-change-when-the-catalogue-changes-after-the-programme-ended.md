---
id: T312
title: A completed programme's record, and the portfolio PDF built from it, change when the catalogue changes after the programme ended
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-26
---

# T312 — A completed programme's record, and the portfolio PDF built from it, change when the catalogue changes after the programme ended

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. A graduate's record is what the product calls read-only, and what she hands a new employer as the portfolio PDF. A catalogue change made after she left rewrites it. An EPA she was entrusted with vanishes, and closed years gain shortfalls she could never have met.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-6.21a, F-6.40a).

## Symptom

- **Step 6.21** (design/baseline/act-6/6.21-1-molefe-record-paused.png). While PAED-012 is paused in Act 6, Dr Molefe's completed record ('You completed your programme on 26 September 2026 … read-only') has no PAED-012 card and no PAED-012 periods. 'Entrustment against Annexure A' reads '14 of 14 EPAs at their exit level', where Act 5 (Step 5.20) read '15 of 15'. Only her PAED-012 trajectory still charts, headed '(no longer in use)'.
- **Step 6.40** (6.40-2-molefe-kgk001-card.png). KGK adds its own item KGK-001 to 11.1 after her graduation. Her record then shows a KGK-001 card under 'Once a year', with 2025, 2024 and 2023 each '0 of 1, 1 short'. Her standing reads '15 at or above · 0 below · 1 with no decision, of 16 EPAs'.
- **Step 6.41** (6.41-1-duplessis-record-after-rebuild.png). Dr du Plessis's withdrawn record gains the same card ('2025 academic year: 0 of 1, 1 short') and reads '16 with no decision, of 16'.
- The portfolio PDF's 'Progress per EPA' is the same read, so a graduation export made after either change prints the rewritten record.
- The runbook's Expects describe this behaviour because they were written from the code. DESIGN's rule does not.

## Root cause

- TraineeQuotaProgress.cs:246-250 reads an ended profile 'as on the last day' for its periods only. The item list at :279-285 is .InForce() (the EPA is active now) plus OwningInstitutionId == null || == profile.InstitutionId (the institution's local items now), whatever the day.
- GetEntrustmentStandingForTrainee.cs:117-121 reads the same live list for the standing and the exit rule, and :145 reads decisions by today's Status.
- MsfSemesterCoverage.OnListOf (MsfSemesterCoverage.cs:50-58), which My progress calls with To: EndedOn, uses the same live list.
- PortfolioPdfService.cs:455-487 calls ReadForProfileAsync and, for EPAs without a target, NotInForce() as of now.
- CurriculumItemsInForce.cs:44-47 says the filter is 'not applied where an item is read as a record rather than as a target'. T158 (2026-09-24) came before T252 (2026-09-25), so no one considered an ended record. DESIGN.md:948-949 says an ended programme is 'read as on its last day'.
- The catalogue cannot answer an as-of question. CurriculumItem has no creation date, and Epa.DeactivatedOn holds only the current pause. Target edits after the end (RequiredCount, per-stage minimums) would also leak into closed periods.

## What to build

Decide, then build. **Recommendation:** freeze an ended programme's EPA list and targets when the end is recorded.

- **Freeze at the end.** When TraineeProfile.Complete or Deactivate records the end, store the list the programme ended with. That happens in CompleteTraineeProfile and DeactivateTraineeProfile, under the ITraineeCreditLock end hold and in the same save. The list holds each curriculum item in force for the profile on its last day, both the national core and the institution's own. Each entry carries its target fields: RequiredCount, QuotaPeriod, MinimumLevelOrder, MinimumLevelByStageJson and ScaleId. A child table of TraineeProfile or a jsonb column; the builder chooses.
- **Every reader of an ended record reads that list** instead of the live catalogue:
  - TraineeQuotaProgressReader: the ended branch, and ReadForProfileAsync when the profile has ended;
  - GetEntrustmentStandingForTrainee;
  - MsfSemesterCoverage for the ended card;
  - the PDF's per-EPA section, whose NotInForce lookup becomes 'on the frozen list'.
- **Codes and titles stay live**, so a correction such as Step 6.14's still shows. An EPA on the frozen list that is paused now is headed '(no longer in use)' (EpaLabel, T255), not dropped.
- **A running trainee is unchanged.** T158's rule, that a paused EPA leaves every progress page, stays for active profiles.
- **STARs on an ended record.** Decide whether the standing's STAR column reads decisions as they stood on the last day. Today it reads Status == Active now, so a STAR that expires after graduation would drop out of '15 of 15'. Recommendation: yes, as they stood on the last day.
- **Docs.** Say in DESIGN § 'My progress once the programme has ended' and in CurriculumItemsInForce's remarks that a closed record reads the frozen list, and narrow T158's 'every progress page' to running programmes.
- **Existing data.** Compatibility is not a constraint: freeze existing ended profiles in the migration from today's catalogue, or regenerate the scenario data.
- **Rejected unless the operator prefers it:** an as-of read (InForceAt(lastDay) plus a new CurriculumItem.AddedOn). It still lets later target edits rewrite closed periods.

## Verification

- [ ] Application test: complete a profile, then deactivate one of its EPAs. TraineeQuotaProgressReader.ReadAsync still lists that EPA's card and periods, and GetEntrustmentStandingForTrainee still counts it in the exit rule (n of n unchanged). An active trainee on the same curriculum does not see it, and T158's tests still pass.
- [ ] Application test: after the end, add a local item at the trainee's institution and change a frozen item's RequiredCount. The ended record lists neither the new item nor the new count.
- [ ] Application test: Complete and Deactivate write the frozen list in the same save as the end, and a refused end writes none.
- [ ] Infrastructure test: a graduation portfolio export made after a post-end pause prints the paused EPA's periods and the same standing as an export made before the pause.
- [ ] Browser, replay Act 6 Steps 6.21, 6.40 and 6.41 against Step 5.20. Molefe's record keeps the PAED-012 card and '15 of 15' during the pause and gains no KGK-001 card, and du Plessis's gains none. Give the runbook owner the new Expects.

## Related

T252 (the ended view; DESIGN 'as on its last day'), T158 (in force on every progress page), T196 and D48 (a pause pauses credit, it does not cancel it), T255 ('(no longer in use)'), T169 (the PDF's per-EPA section), T209/D49 (the end marker), T281 (no credit after the last day), T242 (national and local item overlap). Runbook Steps 5.20, 6.14, 6.21, 6.40 and 6.41. Findings F-6.21a and F-6.40a.
