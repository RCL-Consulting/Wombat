# Flow 03 · Round 2 review (2026-09-29)

Four reviewers (T tokens and CSS; B the build against the code; S states, copy and the cast; A accessibility) read
round 2 (`round-2/`, canvas version `1790666059-feef`: three component boards, 76 state wrappers, Spec and Steps)
against `round-2-ask.txt`, `round-1-review.md`, the brief, DESIGN.md, the seeds and the code at `c3ef5893`. This is
the synthesis. Ids in brackets are the reviewers' own.

**Verdict: accept with changes.** Every state asked for is drawn at 1280 and 390, and every decision from round 1
is drawn as decided. The tokens hold: no literal colour, no undefined class, and all 12 of the Spec's contrast
figures recompute correctly [T2, T4, T10]. Nothing on any board promises an email [S17, B11]. What needs a correction
round is:
- **the ask's own correction 2, which was wrong.** The portfolio review has a named reviewer.
- **words the product would make untrue.** Nobody-else-sees-a-draft; a Submit on a move called Log.
- **cast data that contradicts the runbook.**
- **field orders and labels that are not the seeds'.**
- **a greyed move the code cannot produce.**
- **three accessibility gaps in the markup.**

## Corrections for the canvas (round 3)

- **C1. The portfolio review has a named reviewer.** Round 2's correction 2 was the reviewer's error: `portfolio_review_cpsa` has a required
  "Reviewer" user field, and `sign_off` and `return` belong to `field:assessor_user_id` alone. Step 3.22 names Dr
  Patel. Draw the Reviewer field, the status card "With Mohammed Patel since …" and the result "It is in Mohammed
  Patel's Activity inbox." Its About reads "Credit: none" (`counts_for: []`), not "None until it is reviewed". Keep a
  no-name card ("Waiting for <state label>") only as the generic fallback for a role-held move (the Demo types), on
  the Spec. Seed labels: "Notes for your reviewer", "Portfolio export (file name)". [B1, S1, S12]
- **C2. A draft is not private.** Programme overseers in scope can list a draft, and a named assessor can open one by
  its address (`ActivityService.IsReadableBy`). Replace "Nobody else can see a draft", "A draft is yours alone" and
  "Nobody else can see it" everywhere with "It is in nobody's inbox until you submit it." [S2, B3]
- **C3. Moves by their own labels.** A move's label comes from its key (`WorkflowTransition.LabelFor`): the log's
  move is **Log**, the reflection's is **Record Discussion**, the review's is **Sign Off**. That governs the button,
  the history rows ("Log · Draft → Logged"), the Spec and the Steps board. A person's name goes on the button only
  when the move hands the activity to them: a move out of its target state has a `field:` rule naming a filled user
  field ("Submit to Fatima Khumalo"). The log's Supervising consultant is a user field that receives nothing, so its
  button is "Log". The result is "Logged." for it and "Submitted. It is now …" for a submit (see E4). [B2, S3]
- **C4. The greyed move.** The code greys a move only for a required field its mover cannot write
  (`ExplainUnreachable`), never for a nominee who can no longer be named. Redraw the blocked-move board as that case,
  on an invented type. Its reason is "Submit: needs Entrustment level, which only the assessor fills in." The locked
  assessor case becomes the refusal at submit: a field message on Assessor, "Mohammed Patel can no longer be named as
  an assessor. Choose someone else." In the picker a locked assessor is simply absent (A.6.6); a stored value that
  left the list reads "(not on the current list)". [B4, S10]
- **C5. The cast and the data.**
  - Random Case Analysis on PAED-006 and Chart-Stimulated Recall on PAED-007 are impossible (Annexure A tool lists).
    Put them on EPAs whose list names them. [S4]
  - The 17-EPA picker belongs to Sipho Ndlovu on curriculum 11.2 (Step 6.37), not Pieter du Plessis (15, on 11.1).
    [S5]
  - My activities (full) must be one moment. Ndlovu's re-filed request to Dr Botha is completed (3.13) before his
    reflection is filed (3.14). Make the timestamps agree across boards: the return's date, the decline's, and
    Dr Dlamini's first draft. The returned board shows the reflection as returned, before 3.16's edit. [S8]
  - Say on the board that the 40 rows are an invented volume case; no registrar has 40. [S9]
  - PAED-002's title is known, "Managing common paediatric presentations". Use it. [S12]
- **C6. The forms in seed order, with seed labels.** The Mini-CEX is EPA, Assessor, Date observed, Clinical setting,
  Presenting problem (a one-line `text`), Case complexity. Redraw the boards and A.7.1's focus order in it. The
  reflection has the required "Reflecting on" (`prompt`) field and its seed labels. The DOPS has Entrustment and
  Feedback, not "Assessment". [B9, S6]
- **C7. Not found is also forbidden.** The same page serves an id the reader may not see, and it must not deny the
  activity exists (T101; Step 3.4, Dr Patel). Word it for both: "Activity unavailable" · "This activity does not exist,
  or you cannot open it." Add 3.4 to Steps. [S7]
- **C8. Focus targets.** The summary's links must point at the input (`#<field>-in`), not its wrapper. The same goes
  for the note panel's `#note`. Give the summary an id that every refused field names. [A1, A2]
- **C9. The before-the-programme hint is a predicted refusal.** DESIGN § Alerts (T192): `.validation-message` with
  `aria-invalid="true"`, not a `.field-warning`. [A4]
- **C10. Draw what is missing.**
  - The note panel open (the assessor's board), with focus in the textarea. Decline carries `aria-expanded` and the
    panel's id. "Keep the request" returns the focus to Decline. The refusal's words are today's "Decline requires a
    note." [A6, S12]
  - The draft Cancel dialog (Step 3.20). [S15]
  - A move while it runs. [A10]
- **C11. One wording per message.**
  - Needs you reads the same on Home's card and My activities: "Returned to you by Sarah Botha on <date>. Change it
    and submit again." / "Not submitted yet." Put both in the Spec's table. [S11]
  - "Closed" and "Done", not "Nobody: closed" and "Nobody: done". [S12]
  - Every status headline ends with a full stop. [S12]
  - The check line uses the target state's label ("… and stays Requested until Fatima Khumalo acts on it."): moves
    have no past tense in the workflow. [B15]
  - The Cancel labels follow one stated rule: "Cancel request…" when another person holds it, "Cancel this draft…"
    in a draft. [B15]
  - The locked section's owner line on a closed record: "Fatima Khumalo was to fill this in." [A12]
- **C12. "Your programme" only to its registrar.** `ActivityForm` is shared: the assessor and the committee read "the
  trainee's programme". The Spec gives both. [B10]
- **C13. Keep a place for what the summary card carried.**
  - The form version, which T107's move reason names.
  - T108's and T109's credit warnings.
  - The About card holds them when they apply. [B13]
- **C14. The locked section must not look like an empty state.** `.detail-card--empty` is dashed and muted too. Give
  the locked frame its lock and owner line as its head, and a different ground. [T8]
- **C15. The Steps board is incomplete.**
  - Add 2.43, 3.3, 3.4, 3.6 (the completed card's "Open My progress"), 3.11 (the assessor's words), 3.21, 3.23,
    3.25, 3.27–3.29, 5.24, 6.16 and A.6.6.
  - Add 3.2's history row "Create · — → Draft".
  - Say whether Home's Recent activities stays. It does (flow 05 owns Home), and it is how Ndlovu finds the decline
    in 3.12.
  - State the hint rule: on a form that opens with a date (File it again), the warning shows at once; otherwise
    when the field is left. [S16, S14]

## For the build (step F), not the canvas

- **Save draft on an existing draft** has no command today (T106 item 1). It is a new command that saves data without
  a move, through the date, nominee and tool gates (E3). [B5]
- **Needs you:** restrict the actionable read to `Workflow.TransitionsLeadingOn(state)`, so a cancel-only request drops
  out and the assessor's inbox is unchanged. "Returned" = the last transition entered `InitialState` from another state;
  its note is on that row. A registrar's `/activities/inbox` excludes the subject and creator arms. [B6]
- **Who has it now:** only over the moves that lead on; `field:` arms resolve to a name, `role:`/`scope:` arms fall
  back to the state label. My activities' query must add each row's pinned workflow and nominee name, and paging. [B7]
- **The picker's groups:** "final" means `terminal: true`, not a dead end (every type cancels into one). Add a shape
  field to `ListActivityTypesQuery`; "credits nothing" comes from `counts_for`. [B8]
- **File it again:** honour `?from` only for the source's subject, from a dead end. Copy the fields the author may
  write in the initial state, minus the nominee the next move names and keys the current version lacks. Drop a paused
  EPA. [B10]
- **The rung row:** rungs from the field's `scale_key`; stored value is the order (5 = "4"); extend
  `EntrustmentRungLookup` with the descriptor; "1 of 3 this semester" needs a progress read. Radios are flow 04's. [B12]
- **History:** a real `.clinic-table` in `.table-container`, the note a `<td colspan>`, not a `display: contents`
  grid (roles drop in older Safari and Chromium; it overflows 641–1000 px). SAST with an explicit zone. [A7, B13]
- **The stacked table on a phone:** a visually hidden thead, explicit roles, and `content: attr(data-label) ": " / ""`.
  [A8]
- **Focus on arrival:** the result region or the summary wins over `FocusOnNavigate`'s h1 on arrival from Log an
  activity (the T265 pattern), with an `ActionFocusTests` case. [A2]
- **The late hint's live region:** keep today's always-present `role="status"` wrapper (`ActivityForm.razor:153`). A
  standing info notice has no role. On a repeated load failure, focus the error. [A3, A14]
- **ConfirmDialog** gains a `CancelLabel` ("Keep the request"). The greyed button's handler returns early. The running
  and the cannot states both use `aria-disabled`; DESIGN.md says which owns what. [A5, A10, T5]
- **CSS:**
  - Spacing on the scale, and `2.75rem` for 44 px [T3].
  - Reuse `.details-list--stacked`, `.stack-list` and `.detail-card--emphasis` where `flow03.css` duplicates them
    [T6].
  - Scope the phone block to these pages [T7].
  - The status card's frame is `--border-color` [T8].
  - `.form-section--locked` comes after `.detail-card` [T9].
  - A modifier for the neutral quiet button [T4].
  - The design system's `wombat-root.css` gains `color-scheme` and `--font-display-settings` [T1].
- **Wording the server owns:** `EncounterDateGate`'s refusal becomes viewer-aware (C12).

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| E1 | The portfolio review: a named reviewer, or a pool any reviewer can take? | Named, as the seed and Step 3.22 have it (C1). A pool would need a role actor and changes the reviewers' inboxes |
| E2 | An assessor who can no longer be named | Refused at submit with a field message, as today (C4). Greying Submit in advance needs a new server check for no gain |
| E3 | Save draft on an existing draft's page | In scope: build the save command. A reflection is written over days, and today the only way to keep an edit is to submit it |
| E4 | Button and result words | The move's own label ("Log", "Submit", "Record Discussion"), with "to <name>" only when the move hands it to that person (C3). Results: "Logged.", "Submitted. It is now Requested.", "Cancelled." |
| E5 | File it again: from which states, for whom | Declined only, and only for its registrar. A cancelled draft was her own choice; she starts again |
| E6 | The Mini-CEX's field order | Keep the College's seed order and redraw (C6). Reordering is a seed change for a cosmetic gain |
| E7 | Two requests alike after a re-filing share an h1 (type · EPA · date) | Add " · <nominee>" to the h1, tab and last crumb only when another of the registrar's activities shares the rest |
| E8 | Home's card title | "Needs you", linking to My activities (the canvas's proposal) |
| E9 | A draft saved with no EPA or date | Named "Mini-CEX (Paediatrics) · no EPA yet · no date yet", as drawn |

**The operator's answer (2026-09-29): accept all,** E1–E9 as recommended. Round 3's ask is `round-3-ask.txt`
(C1–C15 with the seeds' field orders and labels, and the EPAs RCA and CSR may be filed on).
