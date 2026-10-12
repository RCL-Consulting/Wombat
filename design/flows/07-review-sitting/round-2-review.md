# Flow 07 round 2: the review

Round 2 (canvas version 1791772499-ebd7, saved in `round-2/`, `83ba4dc7`) drew A at fidelity: Spec, Steps, the chair
before and through the sitting, staging in the line, record and ratify, readers and variants, loading, flow 03's
return link, at 1280 and 390, and the chair's working view at 768. Reviewed 2026-10-12 from three sides, each a Sonnet
agent reading the boards' text: **the code** (`src/`), **the cast** (the runbook, the seed catalogue and the
baseline), and **the design system** (`app.css`, DESIGN.md, BRIEF.md, accessibility). The Spec's four open questions
are answered below. Severity: M must-fix, S should-fix, N nit.

## Decisions for the operator

| | Question | Recommendation |
|---|---|---|
| E1 | **One label for every unstaged line, or two?** The boards print "Not decided at this sitting" on due lines and "Not decided" on optional ones; after D53 the domain has one state for both. | **One:** "Not decided at this sitting", with the decision's sentence under every such line. The dialogs then count all 12 unstaged lines, not 9 ("PAED-002 and 11 others"). |
| E2 | **Who is offered Start?** Today any seated member who may sit, a Coordinator of the institution, or the Administrator (`CallerMayStart`, T194). The boards drew it as the chair's alone. | **Keep today's set,** withheld while the chair cannot act (Q11), with a matching refusal in `StartCommitteeReview` so a stale tab cannot start it. Others read T194's sentence. |
| E3 | **The decisions-due page after an optional sitting.** An annual EPA left unstaged at a semester-1 sitting would read "Not decided" instead of "Due by year end". | **"Not decided" only when the line was closing or its window has ended;** otherwise the page still says "Due by year end". |
| E4 | **An entrustment-only review with nothing staged** could now be recorded and ratified, issuing nothing (the agenda used to force a choice). | **Refuse Record** with "This review decides entrustment only, so stage at least one decision before recording it." |
| E5 | **A remitted appeal** replaces the decision, and the sentence about undecided EPAs would vanish with it. | **Carry it forward** to the replacement decision. |
| E6 | **"Training year 4" in the subtitle** has no source on the review; it would be computed from the programme start. | **The training year at the review period's end.** |
| E7 | **The time stamp** (the Spec's question 1). DESIGN.md:388: "A time: always with its zone". | **"As of 14:02 SAST"** when it is today, **"As of 2026-10-05 14:02 SAST"** otherwise. |
| E8 | **The sticky sitting bar.** A reader's bar wraps to about 100 px; with the top bar, 160 px stays on screen. | **Stick the chair's compact bar only;** readers' bars are static. |
| E9 | **More than one open line at once?** | **One at a time.** Opening another keeps the first's typed draft, shown as "Draft kept" on that line, until staged, cancelled or the page is left. |

Settled from the evidence (no decision needed):

- **Where the sentence lives:** a nullable `UndecidedNote` on `CommitteeDecision`, not on each line (a decision removed
  after Record un-stages a line later, which no per-line copy would cover).
- **Not decided is written at Ratify,** where `CloseAgenda` already writes NotDecided; between Record and Ratify the page
  derives it. Written at Record, a decided-then-withdrawn review would look like a held sitting.
- **"Lerato Molefe's progress"** (question 2): flow 06's registrar page admits Committee member, Coordinator and both
  speciality admins (`ProgrammeTraineeDetail.razor:2`), not the Administrator or an Institutional admin. Draw it for
  those four acting roles only, and not when the trainee is elsewhere; remove it from Nolwazi Mbatha's boards.
- **T324's sentences** (question 3): "Choose a progression category." and "Give the committee's rationale.", parallel to
  "Select an entrustment level.".
- **The bracketed figures** are filled from the seed catalogue and the runbook (C6 below).
- **Re-planning needs no code:** a later sitting in the window plans a NotDecided line again (`AgendaPlanner`).

## Findings

### D53, the domain change (the code)

1. **M. Drop the closing-line gate:** `CommitteeAgendaLine.BlocksRatify` (`:218`); `CommitteeReview.OutstandingClosingLines`
   (`:227`), `EnsureAgendaClosable` (`:247`), `EnsureAgendaSettled` (`:267`, keep its empty-agenda part); `CommitteeAgendaDtos`
   `OutstandingClosingLines`, `RatifyBlockedReason`, `RecordBlockedReason` (`:126-160`); the DTO's `BlocksRatify`;
   `ReviewDetail.razor:911,919`. `IsClosing` stays, for the label only.
2. **M. Remove Defer and Reinstate:** `DeferAgendaLine.cs`; `Defer`, `Reinstate`, `DeferralReason`; `Deferred = 2` (keep
   the other values: `DecidedNamesItsStar` uses 3). A hand-written migration with its Designer and snapshot: Deferred rows
   to NotDecided, drop `CK_CommitteeAgendaLines_DeferredHasAReason` and the column, add `UndecidedNote`. The staging
   refusal "is deferred … Reinstate it" (`StagePendingEntrustmentDecision.cs:163-166`) goes; `ReviewChanged`'s words.
3. **M. `CloseUndecided` refuses closing lines** (`CommitteeAgendaLine.cs:292`): drop the guard.
4. **M. The decisions-due page:** `CommitteeAgendaStatus.Standing` (`CommitteeAgendaDtos.cs:372-383`) reads NotDecided
   where it read Deferred, under E3's rule; rename the Deferred status, column and badge (`GetEntrustmentDecisionsDue.cs:34,114,144,498`,
   `DecisionsDue.razor:88,102`, `BadgeFor.cs:84,94,107`); reword `DecisionsDueText.cs:52-54` and Missed's "nothing
   decided, deferred or on an open review's agenda".
5. **S. The registrar's side:** `CommitteeAgendaText.cs:42,88` print the reason for Deferred; print the note on ratified
   reviews. The portfolio PDF prints no agenda lines (flow 13 decides).
6. **S. Tests to rewrite:** `CommitteeAgendaHandlerTests`, `EntrustmentDecisionsDueHandlerTests`,
   `EntrustmentOnlyReviewHandlerTests`, `ChairMaySitTests`, `CommitteeRequestsAuthoriseFirstTests`, the agenda-line
   Postgres and race tests.

### The page (the code)

7. **M. The quorum from the ticks is new.** Record's reason today is `PanelShortfall` (who may sit), not the ticks. Keep
   it as a higher-priority reason; add "Only the chair is ticked." before the existing quorum rule; the server's
   `HoldsAQuorum` stays the backstop.
8. **M. The evidence reader:** `ActivityPageModel.From` and `ActivityForm ReadOnly` can be reused, and committee readers
   may read the activity (`ActivityReadScope.cs:52-56`); the status card's one action and any transitions must be
   suppressed. **S:** the reader shows the live activity while the snapshot is frozen; say "Now:" with its state when they
   differ.
9. **M. The return link:** `ActivityView` takes no query today. Parse `from` and `at` as integers only and build the
   address from them; show the link only when the reader may read that review (a forged id must not leak a name). The
   page already has its own "Back to …" (`ActivityView.razor:94`): the review's replaces it when present.
10. **M. The progress link needs the profile id** on `CommitteeReviewDetailDto` (`TraineeScopeResolver.PreferredProfiles`).
11. **S. New data:** the training year (E6); the Start dialog's item count (extract it from `StartCommitteeReview`) and the
    MSF campaigns left out (`CountMsfCampaignsOutsideSnapshot`, T173: the boards dropped the sentence); the opened
    line's standing joins four reads that can fail apart: draw its degraded form.
12. **S. Field marking** for domain refusals needs a message-to-field map or typed exceptions; only validator errors
    carry a field today.
13. **S. T329:** no `StatePanel` retry exists; the load error needs a fixed sentence (not `RefusalText`); not found is the
    one `UnauthorizedAccessException`. **T309** needs its domain half first. **T273:** no board draws a retired EPA; add one.

### The design system

14. **M. T356's cause is not what the Spec says.** `.account-row` is fixed with z-index 1, but inside `.sidebar`, which is
    sticky with no z-index, so a positioned `.table-container` later in the DOM paints over it. Give `.sidebar` an explicit
    z-index (10) at ≥641 px and the sitting bar z-index 1; pin it with a stylesheet test.
15. **M. Focus not obscured (WCAG 2.4.11).** No `scroll-padding` exists. Add `scroll-padding-top` for the top bar and the
    stuck bar's measured height (`--sitting-bar-h`), and `scroll-margin-top` on every heading and result that takes the
    focus; a browser check tabbing through 15 lines at 768 and 1280.
16. **S. Disabled buttons** use `aria-disabled` and `.is-unavailable`, never `disabled` (DESIGN.md:1142-1145): Record and
    Stage pending decision stay reachable and their reasons are read.
17. **S. Remove…** is `.btn .btn-sm .btn-outline` (BRIEF A5, DESIGN.md:682,809-813), not quiet-danger.
18. **S. Class names:** app.css has no `__` elements: `.sitting-bar-state` etc. The open row reuses `tr.is-editing`
    (app.css:430-437); a new rule only for the stacked stripe.
19. **S. Reuse:** the progress link is PageHeader's `SubtitleContent`; DESIGN.md:682's "primary page action lives in the
    PageHeader action slot" must be amended for the sitting bar.
20. **S. Stacking:** a container query on the agenda's table container, with `Stack` so DataTable emits explicit roles.
    "Show the other N snapshot lines" is a disclosure button with `aria-expanded`; standalone links get 24/44 px targets.
21. **S. Announcements:** after Refresh, say what changed ("As of 14:31 SAST. Now Decided."); only the quorum line is
    live, not the bar's "Present" copy; the hints are not live; Refresh keeps its DOM identity so focus stays.
22. **S. Dialog safe buttons:** Not yet, Go back and Keep it. Use one word, **Not yet**, except Remove's **Keep it**.
23. **N.** Instrument names as stored ("Case-Based Discussion (Paediatrics)"), or "CbD" where abbreviated; the contrast
    table recomputes exactly; no new colour token.

### The cast

24. **M. The Window column is wrong on 7 lines.** Semester EPAs (001, 002, 004, 005, 010, 012) read "2026 S2"; the nine
    annual ones read "2026" (act-4:350-352; `decisionCadence` in the seed).
25. **M. The pre-graduation agenda:** 13 lines (12 plus the chair's PAED-010), caption "13 EPAs for 2026 S2", and "Already
    decided in this window, so not on the agenda: PAED-001 and PAED-012." (act-5:104-106,162-165). Keep "Decided
    elsewhere" on the Spec only.
26. **M. PAED-001's MSF** reads "None released yet"; only PAED-010 and PAED-012 are Covered (act-4:372).
27. **S. Counts:** PAED-012 holds 3 snapshot lines; the snapshot is 9 frozen lines (8 plus the MSF campaign report): the
    Start dialog and the snapshot card agree. Act 5's twelve rest on one line, Default MSF #1, with both hints and 0
    items; PAED-010 rests on 2. The pre-graduation recount is about 80, not 93.
28. **S. The Review card's facts** (evidence window, Mode, Sits for) have no home before Start: put the window and Mode
    in the bar.
29. **S. The D53 words table** also needs the schedule preview's "must be decided at this sitting, or deferred with a
    reason, before it is ratified" (act-4:191,208; act-5:105).
30. **S. Evidence order and labels:** the snapshot groups by instrument, alphabetically (CBD #13, DOPS #14, Mini-CEX
    #12); staged lines read "Mini-CEX (Paediatrics) #12; Case-Based Discussion (Paediatrics) #13".
31. **N.** PAED-012's sampling line: 2 ratings · 2 assessors · 2 sources (inferred). Step 4.20 ticks DOPS #14. PAED-010's
    MSF record is "Not recorded". The 390 boards and A.7.6/A.7.13 are tested on review #7 (pre-graduation). 4.23's refusal
    can no longer be pressed: say the server's is the backstop.

### The runbook changes D53 brings (for the build's replay)

act-4: 15, 49, 66, 104, 111, 191, 208, 353, 366, 372, 378, 393-411, 435-446, 473-480, 496-518 (4.21 goes), 527-539,
607-616, 632-643, 666, 684, 692, 705, 712, 758-767, 833, 840, 923, 958, 1056, 1111, 1120, 1138, 1195. act-5: 33, 68, 75,
94, 105, 125, 147, 162-165. coverage.md: 156, 363. states.md: 491, 492, 515. A.7.6 and A.7.13 for the new layout.

## Fills for round 3 (C6)

EPA titles: PAED-002 Managing common paediatric presentations · 003 Providing intensive care to children · 004
Managing common neonatal conditions · 005 Providing neonatal care in intensive and high-care settings · 006 Managing
long-term health conditions (LTHCs) · 007 Maintaining and promoting the health and well-being of children · 008
Evaluating and managing neurodevelopmental and behavioural presentations in children · 009 Providing care for an
adolescent patient · 010 Leading and operating within a clinical team · 011 Managing population health challenges · 012
Communicating with and counselling patients, caregivers and healthcare teams · 013 Managing child protection, legal and
ethical aspects of children at risk · 014 Providing palliative and end-of-life care.

Molefe's snapshot: PAED-001 Mini-CEX #12 (Thandi Zulu, 2026-09-23, 5), Case-Based Discussion #13 (David Naidoo,
2026-09-24, 5), DOPS #14 (Mohammed Patel, 2026-09-26, 4); PAED-010 Direct Observation (Thandi Zulu, 2026-10-01, 5) and
MSF record #22 (Not recorded); PAED-012 Mini-CEX #15 (Sarah Botha, 2026-09-28, 5), Case-Based Discussion #16 (Fatima
Khumalo, 2026-09-30, 5), MSF record #23; the MSF campaign report Default MSF #1 (Released, 6 responses). STARs: Act 4
#1–#3; Act 5 #6–#18. Act 5: "Lerato Molefe · 2026 S2", window 2023-01-15 to 2026-12-31. Entrustment-only: Anele
Dlamini, training year 3, 2026 S1, the Neonatal team Clinical Competency Committee; present Thandi Zulu (chair), Sarah
Botha.
