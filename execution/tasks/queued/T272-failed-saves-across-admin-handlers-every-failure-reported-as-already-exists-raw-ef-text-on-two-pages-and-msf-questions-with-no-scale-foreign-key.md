---
id: T272
title: Failed saves across admin handlers: every failure reported as "already exists", raw EF text on two pages, and MSF questions with no scale foreign key
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T272 — Failed saves across admin handlers: every failure reported as "already exists", raw EF text on two pages, and MSF questions with no scale foreign key

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The admin sees the wrong reason, or database text.
**Surfaced:** 2026-09-25, the T254 review.

## Symptom

- **Every save failure reads "already exists".** Ten handlers report every failed save that way: CreateCollege,
  UpdateCollege, CloneCurriculum, CreateCurriculum, UpdateCurriculum, CreateEpa, CreateInstitution, UpdateInstitution
  and others. A grep for "already exists" in catch blocks finds them.
- **Raw EF text.** `SubSpecialityEdit.razor` and `EntrustmentScalesList.razor` show `exception.Message`, so a rethrown
  database exception reads "An error occurred while saving…".
- **An unchecked read-back.** A lookup after a failed save can itself fail, with no guard, in `UpdateEpa`
  (`CodeTakenAsync`), `ScheduleCommitteeReview` (`OpenBindingReviewAsync`), `StagePendingEntrustmentDecision` and
  `CurriculumAdminScope`.
- **No foreign key.** `MsfQuestion.ScaleId` has none, and `CreateMsfTemplate` stores it without checking the scale
  exists.

## What to build

- Map only the matching unique violation (T243's SQLSTATE helper and the constraint name) to "already exists", and
  rethrow the rest.
- Give pages a generic "could not be saved; try again" for database exceptions (a `RefusalText` case).
- Consider the central fix the review suggested: the audit pipeline notes per context that a save was refused, and
  drops pending changes whatever follows.
- Add an ON DELETE RESTRICT foreign key for `MsfQuestion.ScaleId` (a migration) and an existence check in create.

## Verification

- [ ] Each handler maps only its own violation. Tests with an interceptor.
- [ ] The MSF foreign key refuses deleting a scale in use. Postgres test.

## Related

T254, T243, T232, T201.

## Notes

- **T295 replay, 2026-09-26 (C68).** Note, 2026-09-26 (T295 triage; code read, not observed in the replay, suspect from step 4.38): My authorisations, a trainee's page, shows `exception.Message` for a failed load (MyAuthorisations.razor:92) and for a failed certificate download (:121). The download wraps the PDF render and the JS download call, so a failure there puts database, QuestPDF or JSInterop text in front of a trainee. The committee pages use `RefusalText.Of` (T213). This widens the task beyond the two admin pages it names. About 90 assignments of `exception.Message`/`ex.Message` remain in 50 .razor files under src/Wombat.Web/Components, against 16 files that use `RefusalText.Of`. The planned RefusalText case, a fixed 'could not be loaded/saved; try again' for an exception that is not a refusal, reaches only pages that call it. So sweep the pages onto `RefusalText.Of` instead of fixing them one by one, starting with My authorisations. Verify with a bUnit test that a failing load and a failing download on My authorisations show the fixed sentence, not the exception's text.
- **T295 replay, 2026-09-26 (sweep).** T295 states sweep, 2026-09-26 (observed; states home--load-error and profile--load-error). With every table locked past Npgsql's 30 s command timeout, a load inside the circuit fails, and StatePanel's alert prints EF's 'An exception has been raised that is likely due to a transient failure.' It names nothing to do next and offers no retry. Pages: the trainee dashboard (TraineeDashboard.razor:192, `_error = ex.Message`; the other seven dashboards do the same) and My account (Profile.razor:98). This adds two points to the plan. (1) Moving a page onto RefusalText.Of does not fix this by itself. RefusalText.Of (RefusalText.cs:42) returns the Message of every exception that is not a ValidationException. (2) Judging by its message, the exception is the InvalidOperationException that Npgsql's default execution strategy throws around the NpgsqlException or TimeoutException (inferred; DependencyInjection.cs:49 sets no EnableRetryOnFailure). Handlers also throw InvalidOperationException for their refusals. So the new RefusalText case must key on the inner exception (DbException, TimeoutException), or on DbUpdateException, and never on the outer type. The fixed sentence should say what to do, for example 'This could not be loaded. Reload the page, or try again in a minute.' StatePanel (StatePanel.razor:5-8) has no retry, and a Try again that reruns the page's read is worth adding alongside the sentence. Verify with a bUnit test: on a StatePanel page whose sender throws that wrapped exception, the fixed sentence shows and the EF text does not.
- **T295 replay, 2026-09-26 (sweep).** T295 states sweep, 2026-09-26 (observed; state curriculum-progress-rebuild--failed). A rebuild that timed out reads 'The rebuild failed and nothing was changed: An exception has been raised that is likely due to a transient failure.' (CurriculumProgressRebuild.razor:13 and :116, `_error = exception.Message`). The page's own prefix is right. Only the appended text needs the database-failure sentence, for example 'the database did not answer in time; try again'. Include this page in the sweep onto RefusalText.Of, after RefusalText has its database-failure case (see the note on the home and profile load errors).
- **T295 replay, 2026-09-26 (sweep).** T295 states sweep, 2026-09-26 (observed). The same EF text appears on My activities (MyActivities.razor:64), the activity view (ActivityView.razor:265) and My progress (MyProgress.razor:267). It also appears in Schedule review's agenda preview, 'The agenda could not be previewed: An exception has been raised…' (ReviewsSchedule.razor:375, :395). The preview already calls RefusalText.Of, which shows that the sweep onto RefusalText.Of needs the database-failure case first. Screenshots: design/baseline/states/my-activities--load-error.png, activity-view--load-error.png, my-progress--load-error.png, reviews-schedule--preview-failed.png. states.md § 'Holding a read' currently expects 'the raw timeout text' in the alert. Update that line, and retake these load-error screenshots, when this lands.
