# T312 review: tests, pages, docs, runbook (branch t312, 4b65b0f3)

Lens: tests, the Razor pages, the docs, the runbook. Static read only; I did not run the suites or build the worktree.
Severity: M must fix, S should, N nit.

## Verification items against tests

| Item | Test | Verdict |
|---|---|---|
| 1 pause after end keeps card, periods, n of n; running trainee loses it | `EndedCurriculumListTests.AnEpaPausedAfterTheGraduation_...` (:46) | Real. It asserts the codes in order, `EpaInForce == false`, the Semester 1 2026 period (count 3, met), `Paused` empty, `Exit` equivalent before and after, `EpaCount` and `AtExitLevel` of 3. The control is registrar `trainee-2` on the same curriculum (`Ended` null, 2 items, `Paused` = PAED-002). It would fail on master, where the paused EPA is dropped. T158's tests are untouched: the diff only adds tests, and the one Web test changed is covered under R4. |
| 2 added local item and changed RequiredCount not read | `AnItemAddedAndATargetChangedAfterTheEnd_...` (:83) | Real. It asserts no KGK-001 card, `Target == 3` and every period `Target == 3` (the live value is 5), and no KGK-001 in the standing. The control is the registrar reading KGK-001 and Target 5. |
| 3 same save; refused end writes none | `ACompletion_...` (:198), `AWithdrawal_...` (:219), `ARefusedEnd_...` (:244) | Real. The `SaveChangesInterceptor` records, per save that touches a profile, whether `IsActive` and `EndedCurriculumJson` are both modified, and `ContainSingle` requires one save carrying both. The stored list is asserted entry by entry. The withdrawal case also proves a pause before the end and another institution's item are left out. The refused case runs three refusals (a future day, a day before the start, a wrong-institution admin), then calls `SaveChangesAsync` on the still-tracked context, so a list left half-set would be committed and caught. `IsActive` is true and the JSON is null. The refusal is real. |
| 4 PDF after a post-end pause prints the same | `PortfolioEpaProgressTests.AGraduationExport_MadeAfterAPostEndPause_...` | Real. It captures the export before the pause and after it, compares `DetailLines` for every row in strict order, compares `Introduction`, and checks the four period names. It also checks `Heading` and the rendered text. It would fail on master (`NoTarget` would be Deactivated). It has no running-trainee control, but item 4 does not ask for one. |
| 5 browser | none | Carried to T379's replay as the task notes say. |

Also covered: `HerStars_AreReadAsTheyStoodOnTheLastDay` (expired, revoked after, superseded after, revoked before), the EPA-page query (`TheEndedRecordsEpaPages_...`), live codes and titles, a Domain round-trip including the jsonb-rendered shape, and a Postgres test that takes the migration down and up again.

## Findings

### R1 (S) Two user-facing strings still state T158's rule for every progress page
- `src/Wombat.Web/Components/Pages/Admin/Epas/EpaEdit.razor:107` (comment) and the ConfirmDialog `Body` just below it: "it stops being a target on every progress page and dashboard, for every institution that uses it". Pinned by `tests/Wombat.Web.Tests/Admin/EpaEditDeactivationTests.cs:77`.
- `src/Wombat.Web/Components/Pages/Admin/Curricula/CurriculumItemsEdit.razor:69-70`: "no progress page or dashboard shows it/them".
- Also `src/Wombat.Domain/Epas/Epa.cs:43` ("a target on progress pages") and `ActivityReferenceDataService.cs:277` ("no progress page lists it").
- Evidence: the task asks to narrow T158's rule "everywhere it is stated". The change did CLAUDE.md, CUSTOMIZATION, DESIGN and `CurriculumItemsInForce`, but not these. They are now false for a closed record, which shows the paused EPA headed "(no longer in use)". The admin who confirms a deactivation is told the wrong thing.
- Fix: say "stops being a target on a running trainee's progress pages and dashboard; a closed record keeps it, marked no longer in use". Update the `EpaEditDeactivationTests` assertion. Fix the two comments the same way.

### R2 (S) The runbook's Expects and Gaps are now false (the implementer proposed new Expects but did not edit; the task says to give them to the runbook owner)
- `act-6-catalogue.md` Step 6.21 (:498-517).
  - Expect bullets 1-2 ("lists no PAED-012 card", "no PAED-012 row either") are false.
  - The Do sentence "her record does not link it while it is paused" is false (the card links the code).
  - Bullet 3 and the Actual mention "the paused alert" on PAED-012's page. There is none on an ended record now.
  - Gap F-6.21a is resolved by T312.
  - The new Expect is: the PAED-012 card kept, "PAED-012 (no longer in use)", periods as Act 5, the standing "15 ... of 15 EPAs" as in 5.20, the EPA page without the alert, with its periods, STAR and "Open My authorisations".
- Step 6.40 (:909-930).
  - Bullet 1 ("Her fifteen national cards are back") was never gone, so reword it to "unchanged".
  - Bullet 3 ("A KGK-001 card has appeared ... 1 short") is false. The new Expect is no KGK-001 card and a standing of "15 at or above · 0 below · 0 with no decision, of 15 EPAs".
  - The Actual text and the Gap F-6.40a are superseded.
- Step 6.41 (:932-952).
  - Bullet 3 ("A KGK-001 card has appeared on his record too") is false: no card, and the standing "16 with no decision, of 16" becomes "of 15".
  - The Gap note "(the KGK-001 card on an ended record is F-6.40a)" goes.
- "Outcome state", `act-6-catalogue.md:995-996`: "each now also shows KGK-001 (Steps 6.40 and 6.41)" is false. Also `act-6-catalogue.md:36-38`: check that it does not say she holds 15 active records, now true throughout.
- Step 5.20 (`act-5-graduation.md:565`): no Expect is falsified. Its standing bullet will gain, in the new Expects, the line "STAR decisions are shown as they stood on that day, and ratings as they stand today." (the code's wording, not the board's, see R6). Add a "same as 6.21" cross-reference instead of duplicating.
- `coverage.md` rows need no change.
- Fix: edit these steps after the replay (T379 owns the replay), not before.

### R3 (S) Untested branches of the change
- Erasure: `ErasureExecutor.cs:~327-338` now records the list for a profile the erasure ends, and leaves an already-ended list alone. No test (the grep of tests for `EndedCurriculum` finds only the four new test files). DESIGN names the erasure as one of the three recorders. Add a case to the erasure tests: an active profile's erasure writes the list in the same save, and an already-ended one is not overwritten.
- MSF: `GetMsfCoverageForTrainee` was rewired to `ProgrammeItems.ReadAsync`, and DESIGN says "the MSF lines" read the list. No test shows an ended record's MSF card line for a paused or new-item EPA. The EPA-page `ReadsAsInForce` branch for the MSF line is only exercised with `_msfCoverage` unset in the new Web test. Add one Application test (an ended profile, a paused EPA, `Epas` contains it; an added item is absent).
- The take-back path: `ContainSingle` in the same-save tests runs only where no after-end credit exists. `ProgrammeEndCredit.SaveAsync` with `takesCreditBack == true` (replay plus end plus list) is not shown to carry the list in the same save or to put it back on failure. A test with one activity observed after the last day would cover it.
- Step 6.41 (a withdrawn record gaining nothing) is proven only for the saved list, not for the record read after a new item. Test 2 covers Complete only. The reader is shared, so the risk is low. A one-line read in the withdrawal test would close it.
- The Postgres test runs the migration SQL only with national items and one profile. The branch `OwningInstitutionId = p.InstitutionId` (a local item) and the exclusion of another institution's item are not exercised in SQL. A boundary case (an EPA paused after 22:00 UTC on the last day, as `EndOf` says) is covered in the Application test for the withdrawal (07:00 UTC) but not at the edge.
- Staff `EndedRecord` heading (`Staff` true) with a paused item: not tested (the new Web test uses the trainee heading only).

### R4 (N) Web tests: one assertion changed, justified
- `EntrustmentStandingPanelTests` (the `ACompletedProgramme_...` test): "STAR decisions and ratings are shown as they stand today." became "STAR decisions are shown as they stood on that day, and ratings as they stand today." with a reason argument. The change follows the code (`AsOnLastDay`) and is not weakened: it still pins the full sentence. Nothing else was relaxed. Verified by reading the whole test diff (+43 +28 +16 +20 lines).
- The new Web tests are specific: the heading text with the mark outside the link, `a.epa-link span.paused-mark` null, the periods, and no `.epa-page-alert` on the EPA page.

### R5 (N) The pages against the R3 boards and DESIGN.md
- Pass: no inline `<style>`, no Bootstrap, no raw hex. `paused-mark` exists in `app.css:3661`. `epa-page-alert`, `epa-link` and `detail-card` are existing classes. The mark is a muted span after the link, not inside it, as N4 and R3-R05 draw it. The h1 on the EPA page ends in "(no longer in use)" (R3-R06). The PDF `Heading` matches the page.
- `EpaProgress.razor` `Lines` was reordered so the ended branch precedes the `!EpaInForce` break. For a running programme the behaviour is identical.
- The `EntrustmentStandingPanel` now marks any row that is not in force, `InForce="@epa.EpaInForce"`. For a running trainee every row is in force, so there is no regression.
- `EndedRecord.razor:171` holds a long single-line `<h3>` with an inline `@(" ")`. It works and is tested, but it is hard to read. Optional: split across lines.

### R6 (N) Board versus code wording
- R3-R05 draws the standing opening as "STAR decisions and ratings are shown as they stand today."; the code now says "STAR decisions are shown as they stood on that day, and ratings as they stand today." The code is right, since it follows the decision. The board's own caption says "its rows read on the last day". Record the new sentence as the Expect (see R2); the flow 13 build should not "correct" it back.
- `EntrustmentStandingPanel.razor:245`: the `ReviewPeriodTo` branch (a committee view of an ended trainee, where the period end equals `AsOf`) still says "as they stand today", but the reader reads an ended profile's STARs as on the last day regardless of caller (`EntrustmentStandingReader.cs:70`). The staff `else` branch is silent. A rare case. Fix by choosing the wording from `ProgrammeEnd`/the standing's `Ended` and not from `Self`.

### R7 (N) Docs
- True of the code and consistent with each other: DESIGN (the new paragraph: the three recorders, the reads, codes live, the alert removed, STARs on the last day), CUSTOMIZATION §"A deactivated EPA pauses credit", CLAUDE.md's `CreditApplier` bullet, `CurriculumItemsInForce`'s remarks. All four narrow T158 to a running programme.
- CLAUDE.md:204-208: the added sentence runs past the file's wrap width ("... and its STARs read as on the last day. Credit and the rebuild judge each completion at its own moment"). Rewrap.
- CUSTOMIZATION.md:652 (the long "Curriculum progress for trainee X" paragraph) still says the ended reader reads the profile "as on its last day" without naming the frozen list. It is not false, but a pointer to `ProgrammeItems` would help. It also says the rebuild "re-scores their history against today's curriculum", which remains true of the tally and is now a reason the list, not the catalogue, is what the record reads.
- The T312 task file should record the board deviation in R6 and the erasure recorder (it is not in the task text).

## Held
- I did not run the unit, Web or Postgres suites, so "passes" is the implementer's claim. The Postgres test needs `WOMBAT_TEST_CONNECTION`.
- I did not check `ProgrammeItems.cs` line by line (another reviewer's lens): only the call sites, the migration SQL and its doc comment.
- Not verified in a browser (Verification item 5 is carried to T379).
- A profile ended before T209 (no recorded day) has no list and reads the live catalogue. The code and the migration say so. This is a stated limit, not a defect, but those records are still rewritten by the catalogue.

## Summary
Counts: M 0, S 3 (R1, R2, R3), N 4 (R4-R7).
- No M items: every test-type Verification item has a real, specific test with a control, the same-save and refused-end proofs are genuine, and no existing test was weakened.
- R1 (S): the EPA deactivation dialog and the curriculum items page still tell admins a paused EPA leaves "every progress page"; fix the copy and `EpaEditDeactivationTests.cs:77`.
- R2 (S): the 6.21, 6.40 and 6.41 Expects, the Gaps F-6.21a and F-6.40a and the Outcome-state KGK-001 bullet are now false; 5.20 is intact.
- R3 (S): no tests for the erasure recorder, the ended MSF read, the take-back save path or the withdrawn record's read.
