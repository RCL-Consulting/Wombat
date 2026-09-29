# Flow 03 · The build's review (2026-09-29)

Four read-only reviewers read `t342` after the integration (`ba036060`), per `review-lanes.md`. Their lenses:
- **R**: the backend's correctness and security;
- **D**: the pages against the design and DESIGN.md;
- **A**: accessibility and focus;
- **G**: regressions outside flow 03, and how honest the tests are.

The ids are the reviewers' own. One high, ten medium, and the rest low.

## The fix pass (proposed stopping line: everything below, then file the rest)

**High**
- **A1.** The refusal summary's links (`#<key>-in`) resolve against `<base href="/">`, so Blazor navigates to Home. What
  was typed is lost, on Log an activity, the activity page and the note panel. The tests assert only the href.
  - Fix: `@onclick:preventDefault` and a JS focus of the target, keeping the href for no-JS.
  - Add a test that the click focuses and does not navigate.

**Medium**
- **R1.** `SaveDraftAsync` lets the author edit work already handed to a reviewer, on types whose reviewing state
  declares no `editable_by`: the demo `teaching_session`, `qi_project`, `research_output` and `reflective_note` in
  `submitted`, and the legacy `acat`, `cbd`, `dops` and `mini_cex` in `requested`. It leaves no trace.
  - Fix: require that the caller has a move that leads on from the current state (`ActivityWaiting.IsActionableBy`).
  - Add tests on those seeds.
- **D1.** A stored nominee who is no longer offered is still named on the button, the check line and the locked owner
  line (A.6.6).
  - Fix: prefer the form's reported name, or leave `HandsToName` null when the nominee is not in `NomineeDirectory`.
- **A2.** Summary-link targets that cannot take the focus: a multi-choice fieldset, a read-out, a locked line.
  - Fix: `tabindex="-1"`, or point the link at the first checkbox.
- **A3.** Try again on the activity page loses the focus whether the retry succeeds or fails.
  - Fix: one persistent `StatePanel` with `IsLoading`, as My activities has.
- **A4.** Choosing a type on the picker, or "Choose another type", drops the focus to the body.
  - Fix: focus the h1 after a query-driven load.
- **G1.** The assessor can no longer cancel on the four demo generic types (`cancel` is `subject|field:assessor_user_id`).
  - Fix: offer the server's cancel to whoever holds it.
- **G2.** A locked section's owner line falls back to "the assessor" on types with no assessor (`qi_project`'s own
  optional sections, `msf_cpsa`'s and `learner_feedback_cpsa`'s review).
  - Fix: decide the owner per section from its `editable_by`.
  - A section the author owns reads "Not filled in." with no owner line.
  - A `role:` owner is named by role, or not at all.
- **G3.** `ActivityPageModel.Filing` misses a create that is itself the filing (T148). A born-logged `journal_club`
  reads "…'s draft"; a cancelled demo `mini_cex` says "never submitted"; lateness is lost.
  - Fix: fall back to the create row, as `SectionAttributions()` already does.

**Low (in the pass because each is small and in files already open)**
- **R2.** `ReturnOf` counts the author's own move back to draft as a return (`reflective_note` `revise`).
- **R5.** "Submitted. It is now Submitted.": say "Submitted." alone when the target is labelled Submitted.
- **R6.** File it again is offered after a cancel or a reject that was not a decline. Also require that the recorded
  move has a `field:` arm.
- **G4 (and R's note).** The nominee refusal says "can no longer be named" to people who never could be. Use one
  neutral sentence, "<name> cannot be named as an assessor. Choose someone else.", so a lockout is still not revealed.
- **D5.** The subtitle branches: the cancelled draft, and the reflection's and the review's words as drawn.
- **D6.** The draft page's check line gets the late clause.
- **D7.** Only the result's first sentence is bold on arrival.
- **A5.** One `PageHeader` above the branches, so the h1 focused on arrival survives the load.
- **A6.** Always-present `role="status"` regions for the running move and the loading line.
- **A7.** The phone history's `<details>` gets its disclosure marker.
- **A8.** Multi-choice checkbox rows are 44 px at phone width.
- **G5.** The rung row uses `Rungs` only for the field whose `scale_key` matches.
- **G6.** The inbox subtitle becomes role-neutral.
- **G7.** The picker gets an empty state.
- **G8.** The builder's preview heading level.
- **Tests:**
  - A9: the note panel's focus calls.
  - G9: the forged-event guard, called directly.
  - G10: pin the read-only refusal sentence.
  - G11: the stub uses the gate's exact sentence.
  - G12: a Postgres assertion that a trainee's inbox is empty.
  - Every fix above gets its test.

## Filed, not fixed here (proposed)
- **R3.** My activities reads every row, with its clinical text, to serve one page. Collision keys should come from SQL
  grouping, and paging should happen in SQL. Performance.
- **R4.** A save can race a move: there is no concurrency token on `Activity`. Use `xmin`, as `MsfCampaign` has.
- **R7.** E7 names can differ by viewer: an assessor may not see the " · nominee" suffix. Nothing leaks.
- **D2.** The completed card lacks "1 of 3 this semester". It needs a progress read, and belongs with flow 05's
  progress.
- **D3, D4, D8, D9.** Words that differ from the boards:
  - nouns built from the type name;
  - the locked section's pre-filing and closed sentences, and "It holds: …";
  - the greyed move's reason;
  - "7 types" against "(7)";
  - About's "Encounter" label per type;
  - the boards' help lines.
  
  The build's words are coherent, and the runbook is written from the code. File them as one words task, or drop them.
- **D10.** The banner's commit placeholder is filled at the squash. It is not a defect.
