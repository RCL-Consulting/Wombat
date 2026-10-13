# Flow 08 round 3: the check

Round 3 (canvas version 1791874392-bd1c, saved in `round-3/` at `a41cc281`, its text in `round-3/boards-text.md`) was
checked item by item against `round-3-ask.txt` by the main session, from the boards' text, 2026-10-13. The canvas
corrected round 2's boards in place (the file names keep `R2-`; the page is named "Round 3"); the Spec carries a table
of what changed and where. **The design is final.** No fourth round: what remains is one default and figures the build
reads from the code and the seed (CLAUDE.md § Multi-agent workflows).

## Verdict

- **F1–F6: all done.**
  - F1: three "From a Missed row of 2026 S1" frames (1280, 1024, 390). The form opens on 2026 S1 with "2026 S1 has
    ended. A review can still sit for it: sittings are often held after the period ends."; the Agenda reads S1's plan.
    Q7's "current semester" sentence is gone from every board.
  - F2: Show on every filter bar. F3: no ISO text field, no "YYYY", no "2026/". F5: the race refusals are captioned
    "Scratch state: a second tab". F6: "may sit until" appears nowhere.
  - F4: the type is preselected, but the S1 default is drawn as Entrustment-only (the Spec flags it). **Settled from
    the code:** `CommitteeReviewTypes.DefaultFor` is the first allowed type, so a general panel defaults to Annual
    progression in both semesters; Entrustment-only only before a panel sitting as a College body. The build moves one
    radio.
- **M1–M10: all done.**
  - M1, M2: "S2 has not ended" falls only on PAED-001, 002, 004, 005, 010, 012 rows; "2026 has not ended" only on 003,
    006, 007, 011, 014, 015. No annual or optional EPA reads Missed.
  - M3: By EPA's counts per state, the fold open at 4.10, 4.34, 4.37 and 4.2 (Missed 5 on the six).
  - M5: Period offers 2025 S1 to 2027 S2. M6: Botha is named only in the warning and the result. M7: "There is no page
    at this address.", as `NotFound.razor`.
  - M8: 768 boards for Decision panels, Decisions due and Committee reviews. M9: a 1024 board, the Agenda under the
    fields. M10: the Steps board's 4.1 and 4.2 carry their checks, and 4.2 presses a Missed row's Schedule.
- **S1–S8: all done, with placeholders the build fills** (the Spec's Flags):
  - EPA titles not in the brief are drawn "[PAED-0nn title, as seeded]": the build reads them from the seed.
  - NothingToDecide's refusal is a placeholder. The handler's sentence is "{panel} decides no EPA on this trainee's
    curriculum, so an entrustment-only review before it would have nothing to decide."
    (`ScheduleCommitteeReview.cs:229`).
  - S1's annual and optional agenda lines are drawn as expected; the build shows what `AgendaPlanner` gives.
  - The neonatal panel's name, the Administrator's name and two people are placeholders; the build takes the runbook's.
- **Step counts (S8):** the Today column is recounted. A at 1280 matches today on most journeys and wins on 4.8 (refused
  before Create, not after) and 5.1/5.2 (the Trainee filter replaces reading past 45 rows).

## For the build

- The Spec's "Data the build must add" list, plus round 2's (`round-2-review.md`, "Data the build must add").
- The runbook steps outside this flow that change: 2.35, 2.37, 4.14, A.1.14, A.7.5–A.7.8, A.7.14, every "Formative
  only" and "Mode" (`round-2-review.md` § Should fix, last item).
- The proposed app.css "Committee calendar" section is in the boards' helmet CSS, every phone rule scoped to
  `.committee-calendar`.
