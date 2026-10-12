# Flow 07 round 3: the check

Round 3 (canvas version 1791774971-ed50, saved in `round-3/`, its text in `round-3/boards-text.md`) was checked item by
item against `round-3-ask.txt` by the main session, from the boards' text and markup, 2026-10-12. The canvas corrected
round 2's eleven boards in place (the file names keep `R2-`); the Spec carries a table of what changed and where.
**The design is final.** No fourth round: what remains is markup detail and figures the build reads from the data
(CLAUDE.md § Multi-agent workflows).

## Verdict

- **E1–E9 and the settled points:** all done.
  - E1: Record "Records 12 EPAs as not decided: PAED-002, …, PAED-015"; Ratify "PAED-002 and 11 others …". "Not
    decided" alone is gone.
  - E2: Naidoo's Scheduled bar has Start; Mbatha reads T194's sentence; with the chair unable to act, nobody gets Start.
  - E4: the entrustment-only refusal is beside Record, at 1280 and 390.
  - E7: both forms.
  - E8: readers' bars are static; at 768 Present is hidden.
  - E9: "Draft kept".
  - E3, E5, E6: on the Spec.
  - The progress link is removed from Mbatha's frames.
- **Corrections 7–31:** 23 done; 2 partial in the markup only (17, 20); 1 left open by the canvas (21's "No change"
  wording, settled below).
  - 17: Remove… is drawn `.btn .btn-sm` at 1280 and 768 and `.btn` at 390. On the canvas that matches Stage's look;
    the Spec names `.btn .btn-sm .btn-outline`.
  - 20: the stacked lines carry no `role="table|row|cell"` in the markup; the Spec requires it. `aria-expanded` is
    drawn.
- **Fills:** all filled. Still bracketed, as the Spec lists them:
  - the Direct Observation's item number;
  - MSF #23's state at Start;
  - Act 5's day D;
  - PAED-010's two pre-graduation lines;
  - the order of STARs #6–#17;
  - the entrustment-only review's staged decisions;
  - the two "cannot act" and "cannot sit a quorum" reasons;
  - du Plessis's moved portfolio review;
  - the rationale typed at 4.20;
  - the formative check-in's item numbers.
- **The recount:** pre-graduation goes from 93 to 80. Act 4 stays at 36 / 54. Steps 4.28–4.32 each lose 29–35
  presses (their deferrals go).

## Settled here (the integrator's reading of the operator's decisions)

- **Refresh on a reader's bar while the review can still change: Scheduled, In progress and Decided.** It does not
  show once the review is Ratified or Closed.
  - The boards disagree: Naidoo's Scheduled frame says "no Refresh before Start", while Botha's another-day frame
    (Scheduled) has one.
  - A Scheduled review changes when anyone starts it, so Refresh applies there too (C4, E7).
- **Refresh with nothing changed reads "As of 14:31 SAST. No change."**, as the Spec proposes.
- **The line beside an available Record** ("Recording fixes the 3 staged decisions, and records the 12 EPAs not
  staged as not decided.") was not asked for. It is kept as a hint and is not a live region; the dialog still says
  the same in full.

## What the build carries (not a design question)

1. **D53 is a domain change** (round 2 review's findings 1–6), so it gets the full review pipeline:
   - `UndecidedNote` on `CommitteeDecision`;
   - NotDecided written at Ratify through `CloseAgenda`, and `CloseUndecided`'s guard dropped;
   - Defer, Reinstate and `DeferAgendaLine` removed, with a migration (the check constraint, existing Deferred rows);
   - the entrustment-only refusal (E4), at the server as well as on the page;
   - the decisions-due page's E3 rule;
   - the registrar's reviews page and the schedule preview's words (the Spec's D53 table).
2. **The markup the boards leave out:**
   - the stacked agenda's explicit table roles (20);
   - Remove… as `.btn .btn-sm .btn-outline` (17);
   - `.sidebar` z-index 10 at 641 px and wider, and `.sitting-bar` z-index 1 (14);
   - `html { scroll-padding-top: calc(56px + var(--sitting-bar-h, 0px)) }`, with the bar's height measured in
     `wombat.js` (15).
3. **No pronouns in any shipped word**, including the DESIGN.md rule text: the Spec's "her sticky sitting bar"
   becomes "the chair's sitting bar".
   - Pronouns remain only in board captions (canvas notation).
   - The runbook's typed sentence "To be decided at her final review." is data the operator types, not a Wombat string.
4. **The brackets** are read from the data at replay (above), and the reason sentences from the code that refuses.
5. **The runbook:**
   - Step 4.21 goes.
   - 4.15–4.32, 4.50, 5.3–5.6, A.7.6 and A.7.13 take the Steps board's Expects.
   - The recount is as above.

## Files

- `round-3/project/*.dc.html` (11 boards), `canvas.json`, `r2.css`, `ds/wombat/tokens.json`;
  `round-3/boards-text.md`.
