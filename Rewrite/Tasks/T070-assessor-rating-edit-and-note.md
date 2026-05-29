# T070 — No assessor rating-edit / assessor-note surface in the Rated state

**Status:** open (found during Act 3 play-through, 2026-05-29)
**Severity:** Medium — assessors cannot adjust ratings or record feedback before completing.
**Surface:** `src/Wombat.Web/Components/Shared/Activities/ActivityDetail.razor` (+ ActivityForm,
ActivityService update path).

## Symptom

After an assessor Accepts a submitted activity (state → Rated), the activity detail page renders
the submission as **static read-only** (DOM: 0 editable, 12 disabled fields) with no assessor-note
input. The assessor can only click **Complete**. Scenario Act 3 Step 3.5 — "assessor adjusts the
trainee's Communication rating from 4 to 3, adds an assessor note, then Completes" — **cannot be
performed**. Verified during the play-through.

## Desired behaviour

In the Rated state, for the bound assessor (`field:assessor_user_id`), render the rating fields
editable (reuse `ActivityForm` with `ReadOnly=false`, gated on actor + state) plus an assessor-note
field, persisted via the existing draft-update/transition path, so the final `DataJson` (and the
credit decision, which reads `overall_level`) reflects the assessor's judgement, not only the
trainee's self-rating. Non-bound users stay read-only.

## Verification

As the bound assessor in Rated state: change a rating, add a note, Complete; stored `DataJson`
shows the assessor's values and the credit row reflects the assessor-adjusted `overall_level`.
Re-run Act 3 Step 3.5 as written.
