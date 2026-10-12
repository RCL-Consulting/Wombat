# Flow 08 round 2: the review

Round 2 (canvas version 1791828895-937e, saved in `round-2/`, `716adffc`) drew A and the panels at fidelity: Spec,
Steps, Decision panels, a panel, Decisions due (also 768), Schedule a review (also 768) and Committee reviews, at 1280
and 390. Reviewed 2026-10-12 from three sides, each a Sonnet agent reading the boards' text: **the code** (`src/`),
**the cast** (the runbook, the seed and the baseline) and **the design system** (`app.css`, DESIGN.md, BRIEF.md,
accessibility). Severity: M must-fix, S should-fix, N nit. Code is cited at `716adffc`.

## Decisions for the operator

| | Question | Recommendation |
|---|---|---|
| F1 | **Q7 cannot do what it says.** An S2 review never decides an S1 semester EPA: the planner plans only the sitting's own windows (`AgendaPlanner.cs:210`) and a STAR counts only in the window its sitting falls in (`DecisionWindowRecords.cs:125`). Scheduled for S2, a Missed S1 row stays Missed, and Schedule offers again. | **Keep today's rule:** Schedule on a Missed row opens the form on its own ended semester, which is the only review that clears it, with "2026 S1 has ended. A review can still sit for it: sittings are often held after the period ends." Reverses Q7. |
| F2 | **Show or Apply** on the filters. C9 said Apply; the boards drew Show, the word every other list's filter bar uses (flow 06, review 33). | **Show,** one word for every list. |
| F3 | **Dates typed as ISO text** (PROPOSED) drop the browser's date picker and its format check, and add a parse refusal with no words. The fault was only the picker's locale display ("2026/01/01"). | **Keep the browser's date input;** ISO everywhere a date is shown, not typed. |
| F4 | **Review type preselected?** The boards preselect none, which adds a refusal the command never raises (its type is nullable). | **Preselect** the default, and the only type where one is allowed (a panel sitting as the neonatal committee). |
| F5 | **Steps 4.8 and 4.13 stop testing the refusals,** since Create and the committee select are no longer offered while the seat is held or a review is open. The server still refuses a race. | **Keep the refusals in tests** (handler and bUnit) **and as scratch states** in `states.md` (a second tab), and let 4.8 and 4.13 check the warnings shown first. No runbook race steps. |
| F6 | **Q9's line** "reviews for it may sit until 31 December" is not a rule: nothing gates when a review sits; the 31 December window governs what counts. | **"The College's semester 2 ends in November; its window runs to 31 December, so December's work counts."** |

Settled from the evidence (no decision needed):

- **Cadence and titles from the seed** (`Persistence/Seeds/paediatric-epa-v11.1.json`; `act-4-annual-review.md:25–30`):
  semester EPAs PAED-001, 002, 004, 005, 010, 012; annual 003, 006, 007, 011, 014, 015; optional (as opportunity
  allows) 008, 009, 013; PAED-004 and 005 the neonatal committee's. Real titles from `act-1-setup.md:342`; PAED-001 is
  "Providing paediatric emergency care to children". The brief named neither; it is corrected.
- **Not found** on a panel is flow 01's page, which says nothing about records elsewhere (`Pages/NotFound.razor`).
- **Row names** keep `ReviewRowNames`' form ("Open the 2026 S2 review of …", "… formative review of …"), redrawn.
- **The list's Type** reads "Formative check-in" (C8): `FormativeTypeShortLabel` and its tests change.
- **The duplicate refusal** gains the trainee's name and loses the doubled word; the handler and its tests change.
- **Data the build must add:** member names and the speciality on the panels list; a panel's open reviews (with
  `MayOpen`); the held seat for the schedule page (a query independent of the preview); the trainee line's holder type,
  date and an open check-in read; training year on the trainee picker; `?created=` after a new panel; `return=` for
  Cancel; a label parameter on the shared pager. Each is S to M.

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | cast, code | Windows drawn on the wrong EPAs (001–006 as semester); 003 and 006 drawn Missed in 4.2 | Redraw from the seed sets above, every board with rows |
| M2 | cast, code | Annual Not scheduled rows say "2026 S2 has not ended." | "2026 has not ended." (`DecisionsDueText.cs:67`) |
| M3 | cast, code | By EPA's counts are the initial ones on every board | Counts per state; draw the fold open at 4.10, 4.34, 4.37 and 4.2 (S1 caption) |
| M4 | code | Q7's sentence is false (F1) | As F1 decides |
| M5 | code | The schedule page's Period offers 4 semesters; Decisions due's 6 (`CommitteeReviewPeriods.Around`) | Six, 2025 S1 to 2027 S2 |
| M6 | code | The can-no-longer-sit board still offers Dr Botha as Chair | Left out of every control |
| M7 | code | The panel's Not found writes a sentence about other institutions | Flow 01's Not found |
| M8 | design | Committee reviews, Decision panels and By EPA stack only below 641; at 768 the 7-, 5- and 8-column tables split or scroll (flow 06's lesson) | `clinic-table--stack-wide` on all three; draw each at 768 |
| M9 | design | The schedule grid keeps a 22rem aside from 901 px, leaving the fields about 211 px wide | Stack below 1100 px (the dashboard grid's line) or a container query; draw 1024 |
| M10 | cast | Steps 4.1, 4.2 lose checks (20 a page, the row sentences, D42's start, 75 rows); 4.2's Do never presses the Missed Schedule | Restore them; play F1's Schedule in 4.2's Do |

## Should fix

- **Data and states:** two-panel heavy case only half drawn (S4 cast): one trainee line per seat, the two-option Panel
  select, the neonatal rows' sentences; S1's three types with their help and the neonatal panel's one (S15 code); the
  current-registrars-only picker (5.29); the Trainee-holder note on the new page (S16 code); `NothingToDecide`.
- **Committee reviews' heavy rows** contradict the runbook (S3 cast): after A.1.14 Ndlovu is three pseudonym rows; no
  Dlamini check-in; Entrustment-only only in an S1 period.
- **Addresses** whole and `.local`; van Rensburg on `sun.wombat.local` (S2 cast).
- **Words:** refusals for the evidence window and a missing name, speciality or institution (`WithMessage`, code 27,
  28); "never the exception's text" applies to unexpected exceptions only, a handler's Wombat sentence still shows (code
  29); the subtitle not "each semester" (annual EPAs); "the agenda is read again when it opens" is untrue (planned at
  Create); the race summary links a short clause, not the paragraph (design S7); the Status filter keeps today's order
  (code 18); rows sort by surname, which the DTO must carry (code 13).
- **Steps 5.1, 5.2, 4.6, 4.9** keep their quoted checks (cast S5–S8); the trainee line's three states are played in
  4.9/4.12 and 4.49/A.1.10.
- **CSS (design S1–S8):** reuse `.rung-legend`'s fold and the global `.fold-show`/`.fold-hide`, `.scale-choices
  .form-check` for the type radios, `.form-check` for seats (rename the class off "seat" where it serves Scope),
  `.is-running` for busy, `.muted` for the opening; fold summaries 44 px at every width; 44 px phone controls on these
  forms; scope the new phone rules to these pages (they would change flows 03, 05 and 06); one narrow live region in
  the agenda aside, no `role="alert"` inside it; a group refusal tied by `aria-describedby`, its summary link focusing
  the first radio, a forced-colours stripe, no padding shift; no summary link to a disabled control; no static inline
  styles; Cancel while busy not a followable link.
- **Accessibility nits:** label-in-name on "Open check-in #6" (design S9); "Scheduled" one badge colour on both lists
  (S10); "1 seat"; "Schedule a review" the same words in the header button; "—" read as "None".
- **Today column** of the step counts overstated (code 22): recount.
- **Out-of-flow runbook steps** that change (cast § 3): 2.35, 2.37, 4.14 (the panels list's Scope and Members), A.7.5–
  A.7.8 and A.7.14 (stacked lists, the new page), A.1.14 (row shape), "Formative only" and "Mode" in Act 4, Act 5 and
  the appendix; `coverage.md` gains `/committee/reviews/new`; `states.md` § Committee rows follow. These are the
  build's runbook lane, not round 3's.
