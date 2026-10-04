# Flow 05 · Round 2 review (2026-10-04)

Four reviewers (T tokens and CSS; B the build against the code; S states, copy and the cast; A accessibility) read
round 2 (`round-2/`, canvas version `1791080399-d995`: five component boards, 112 state wrappers for 56 states at 1280
and 390, Spec, Steps and `flow05-r2.css`) against `round-2-ask.txt`, `round-1-review.md`, the README's decisions (V1 as
reviewed; Q8, Q9, O1, O2), the seeds, the runbook's T350 Actual lines and the code at `644e495b`. This is the synthesis;
every must-fix was checked against the source it cites, and one was dropped for it. Ids in brackets are the reviewers'.

**Verdict: accept with changes.** Every state is drawn at both widths, each wrapper matches its name, and Pick, Q1–Q9,
O1, O2, B1, C2–C8 and N1–N6 are drawn as decided [S holds]. Tokens only, no literal colour, every contrast figure on the
Spec recomputes [T holds]. Headings, landmarks, captions and row headers are right [A holds]. The EPA id is the right
key, the paused sentence is true, and Home's furthest-short rule matches 3.50 and 4.39 [B holds]. What needs a
correction round is:
- **the standing panel is not whole** (C1 of the ask): six departures from the built table, and three wrong or
  placeholder cast values;
- **built words redrawn or left as slots**: part-way, not started, December's rows, MSF's line, the retry rule;
- **cast rows that are not the runbook's**: Ndlovu's five at 3.12, PAED-006's title before 6.14;
- **two CSS rules that lose or break** (the index caption, the 44 px link at 390) and a chart wider than its card;
- **a Spec whose reads, and its "cannot disagree" claim, are not true of the code**;
- **accessibility gaps**: the loading status, the EPA page's loading title, two targets under 44 px, the December row.

## Corrections for the canvas (round 3)

- **C1. The standing panel, whole, as built** (EntrustmentStandingPanel.razor:48-160). Draw on R2-C-Progress:114-131
  and the panel data at :200-264:
  - the opening paragraph whole: the year sentence, "Each target is the level the curriculum sets for training year N:
    Annexure A's, for the College's EPAs.", "The exit level is the level each EPA must reach to finish the programme. A
    level is compared only on the ladder the curriculum sets for that EPA." (:48-79); not started reads "Your programme
    starts on <date>, so the targets shown are for training year 1." (:53-56), not "You are in training year 1" (:278);
  - the summary and exit rule as a `dl` under "STAR decisions against training year N targets" and "Exit rule" (:81-87);
    the exit rule ends "Not yet: PAED-002, PAED-003, …." whenever unmet, and with KGK-001 "The institution's own EPA is
    not part of the College's rule, so it is not counted." (ExitLine, :209-237);
  - the info line "For information only. Recording a Graduate decision or completing the programme does not check the
    exit rule." (:88-90), not "… the exit rule gates nothing." (:118); the built caption, visible (:95), not :121's;
  - the Exit level cell is the level over "Reached" / "Not yet" (:144-147); empty STAR and rating cells read "None";
    a local row adds "The institution's own EPA; not in the exit rule" under its name and, with no year map, "Exit
    level; no year N level set" under its target (:118-131);
  - Latest rating's second line is RatingWords whole: "Direct observation; at or above the target", "Conversation; at
    or above the target" (:260-267), on every row :214-264 now drawing the source alone;
  - Not comparable adds its reason, "Decided on <ladder>, not <scale>." (:249-254), with specimen names for :238, :241;
  - the cast: Dlamini's latest PAED-001 after 3.28 is the Clinical Case Analysis, "4 · Encounter 2026-09-27", "Case
    analysis; at or above the target" (newest ObservedOn, GetEntrustmentStandingForTrainee.cs:182; 3.27 Actual: CCA
    `D−6`, Botha; 3.28: rung 4), not the CBD's 3b (:221); her PAED-004 is "4 · Encounter 2026-10-01" (Khumalo, 3.28),
    not "[rating]" (:222); Ndlovu's PAED-002 is "3a · Encounter 2026-09-13" (3.13), not "[rating]" (:264).
  The four decided changes stay: stacked, 44 px links, ISO dates, the name linking to the EPA page. [B1, B2, S1–S6,
  S9, S10]
- **C2. Part-way and not started in today's words** (MyProgress.razor:96-130, ISO dates). The part-way Alert is "You
  started the programme on <date>. That was part-way through the semester, so under the College's rule no semester
  target applies until Semester 1, 2027, which starts on 2027-01-01. Encounters on those EPAs before then stay in your
  portfolio as evidence, but do not count towards a later semester's target." plus the yearly pair; :272-276's "Your
  encounters are counted" contradicts it. Not started: "Your programme starts on 2027-01-15. Your first semester targets
  are for Semester 1, 2027. Your first yearly targets are for the 2027 academic year." in place of the slot (:281). A
  waived cell reads "No target this semester · 2 recorded · targets start with Semester 1, 2027" (:477-483), so R3's
  start is on the row; This period's waived figure reads "none apply yet". Not started's window column and captions
  read the window containing today, Semester 2, 2026 and the 2026 academic year (QuotaWindow.For), with "No target yet ·
  targets start with Semester 1, 2027" in the cell, not "Semester 1, 2027" as the column (:278-282). [S7, B14, B15]
- **C3. December keeps the figure.** Home's December row (R2-C-Home:172) reads "0 of 3 this semester · 3 more;
  encounters in December still count towards Semester 2, 2026.": the build keeps the count in the head and replaces
  only "n more by" (MyProgress.razor:441-470). A yearly row names its own window, "1 more; encounters in December still
  count towards the 2026 academic year." (`current.Name`), not Semester 2 on every row (R2-C-Progress:191). [A6, S11,
  B20]
- **C4. The EPA page's lines as the code gives them** (R2-C-Epa:149-179):
  - other-scale: "At the minimum level when observed: 0 of 2", the line being MinimumLevelReachedCount of Count and an
    off-scale completion counting towards Count (CreditApplier.cs:234-247), with no parenthesis (:179);
  - paused: no MSF line (coverage lists items in force only, MsfSemesterCoverage.cs:55; CardLine returns null) and no
    "Last encounter" (it is the credited tally's, and the one completion credited nothing): N6 says no count (:174);
  - MSF is CardLine whole, this semester and the one before, newest first: "MSF in Semester 2, 2026: … MSF in Semester
    1, 2026: …" (MsfCoverageText.cs:15-31), as C6f's "never shortened" decided (:149, :164);
  - no line where the build has none: drop "Last encounter: none yet" (:167, 169, 171, 177); a STAR with no expiry reads
    "Issued 2026-10-03" alone, no "Expires: No expiry" (:157); KGK-001's level line is the ask's "Minimum 3a" (:177).
  [B3, B5, B6, S13, B14]
- **C5. The paused mark where Q6 puts it.** R2-E-paused draws the trajectory in its EPA mode with the h2 "Rating
  trajectory" and no mark; draw it with "(no longer in use)", as R2-T-paused-heading does. The tab is the h1's words, so
  it carries the mark; the crumb stays the code. Say both on the Spec (R2). [A12, S18]
- **C6. The cast.** At 3.12 Botha has not completed the re-filed Mini-CEX (3.13), so PAED-002 is 0 of 3 and Ndlovu's
  five are PAED-001 to 005 (shortfall, then code): `decline` (R2-C-Home:198) must not reuse 3.16's `ndlovuShort`
  (:183). PAED-006 is "Managing long-term health conditions (LTHCs)" until 6.14 saves "in children" (6.14 Actual): fix
  R2-C-Home:169 and R2-C-Progress:160 for with-stars, first, counting, standing and exit-met. [B7, S8]
- **C7. CSS that loses or breaks.**
  - `.index-caption` (0,1,0) loses colour, size, padding and alignment to the built `.clinic-table caption` (0,1,1,
    app.css:312): write `.clinic-table .index-caption`. `.trajectory-table caption` ties it and wins by order only: it
    must sit after app.css:312 in the merge. [T2]
  - The 390 rule makes `.activity-block-link` and its kin `display: flex; flex-wrap: wrap` (flow05-r2.css:175-177), so
    a short name and `.activity-link-to` sit side by side ("…2026-09-13from Thandi Zulu"; app.css:2640 says the second
    line). Add `flex-direction: column; justify-content: center; align-items: flex-start`. [T3]
  - "Minimum 5" / "Min 5" in `warning-color` (flow05-r2.css `.trajectory-chart-minimum-label`) breaks "never words" in
    tokens.json and DESIGN.md:191: draw it in `text-color`. [T4]
- **C8. Focus and announcements.**
  - The loading status is always on the page, empty once the read answers (DashboardFrame.razor:17; ActivityView:38);
    R2-C-Home:57, R2-C-Progress:48 and R2-C-Epa:63 add it with its words inside the loading branch. Draw the empty
    `<p role="status">` in every state; Spec § 5 states the rule; "Loading My progress." and "Loading this EPA." are new
    words. [A2]
  - The EPA page while it loads, as flow 03's ActivityView (:276): h1 "Loading this EPA", crumb "Loading…", the status
    filled; once loaded the h1 takes the focus again, so the h1 FocusOnNavigate focused is the loaded page's. Not "EPA"
    (R2-C-Epa:181-189). This closes Spec § 8's open point. [A4, B23, S22]
  - Retry, as built: the button is gone while the read runs (the skeleton) and the answer takes the focus
    (StatePanel.razor:100-104). Spec § 5's "keeps the focus … 'Trying again…'" goes; the ask asked it, the code has
    no such state. [A1, B14]
  - `#standing-h` (R2-C-Progress:113) gets `tabindex="-1"`, the target Spec § 5 names; failing again, the alert takes
    the focus (StatePanel's FocusFailureAfterRender). [A3]
- **C9. 44 px at 390.** "Open My authorisations" on the EPA page (R2-C-Epa:101, a bare link in a `p`) becomes a 44 px
  block like `.epa-link`; the empty state's "Log an activity" (:132, `.btn` at 2.25rem, bundle.css:378) gets 2.75rem
  below 641 px. The Spec lists every 44 px lift: flow05-r2.css:175-177, these two, and the built ones the boards rely
  on (`.alert-row .btn`, `.dashboard-card-footer .btn`, the breadcrumb's parent). [A5]
- **C10. Spec § 6, the reads, true of the code.**
  - "My progress's counts … Exists … unchanged" is false for the index: TraineeCurriculumProgressDto carries no cadence,
    opportunistic flag, local flag or exit level; say "Exists, gains four fields". [B11]
  - The EPA page's activities are not "through T343's query": T343 (queued, P3) is SQL paging and adds no EPA filter,
    and ListActivitiesBySubjectQuery has no EpaId (:23-27). Name it new (an EPA filter on that query, its E7 names
    taken over the whole list), and say "finished" means a terminal state, so a Declined or cancelled request is never on
    the page. [B10]
  - The trajectory read gains assessor names, activity names, the stage per encounter, the stepped minimum, the exit
    level and the other scale's label (TrajectoryPointDto has none); EpaTrajectoryDto is shared with ReviewDetail; "in
    the 2026 academic year" needs From/To, which MyProgress does not pass today. [B13]
  - The last paragraph's claim waits on E2: CreditTargetResolver does not compare levels (CreditApplier does), and "n of
    m" is a stored per-semester tally while the table is per year. [B4]
- **C11. The Spec's words and open points.**
  - List as NEW every line with no source: "No rating yet." (the chart's built empty is "No observations to plot."),
    "Your EPAs", "Why it is not counted", the December "Semester 2, 2026 counts encounters observed in December." in
    place of the ends line, the ended Home's "No STAR yet.", the trajectory key's lines. [B14, S21]
  - The trajectory summary is the ask's template: "2 ratings in the 2026 academic year, from Sarah Botha and Mohammed
    Patel. …"; "so far" is for one rating; the "1 on another scale" clause is new words (R2-C-Trajectory:148). [S15]
  - § 8 closes: the values are C1's, the words C1's and C2's, the loading title C8's, the denominator C4's. § 1 and § 7
    lose the first person ("my words", "which I have not done here"), and the phone heights are measured, as the ask
    said, not "set generously". [S20]
  - One apostrophe, straight, in app strings: R2-C-Epa's level line ("STAR’s"), "College’s", the WaysIn card's
    "Dlamini’s". [S16]
- **C12. The Steps board.** Quote the runbook where a cell says "Not quoted in the brief": 3.12, 4.15, 4.39, 4.42, 5.8,
  A.4.6; write A.7.6, which this flow changes (an unscaled chart at 390; Gap F-A.7.6a, T323). Route cells for 3.7, 5.26,
  6.15, 6.24 and 6.27 add `/portfolio/progress/{EpaId:int}`; 6.15's Do opens PAED-012's row. Record the gaps the design
  closes (F-2.39a/T306, T280 at 3.12, T328 at A.7.3, T304 at 6.37) and 3.12's second half (Recent decisions lists only
  the Declined row until 3.13). [S12, S19]
- **C13. The Spec's tables.** Add muted-text on warning-bg (4.79, "Today" over the shading); the rung gridlines,
  border-color on surface 1.30 and on warning-bg 1.23, marked decorative; the shading on the band, 1.05, carried by the
  dashed line. Drop `.progress-row--waived` (used on no board) or draw it; say paused is `tr.is-paused` with
  `.paused-mark`, not `.detail-card--paused`. [T5, T8]

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| E1 | The wide chart is 900 px from 641 px up (`.only-wide`, app.css:2984), but the card is that wide only from ~1254 px (sidebar 250, gutters, card padding): at 1100 it spills ~150 px with `overflow: visible`. The 326 chart overflows the committee page's nested card at 390 (274 px, A.7.6). Q5 said "never scaled" [T1, S12] | Choose the size by the card, not the viewport: a CSS container query on the trajectory card draws 900 where the card holds it and 326 below, no script. Under both, a named, focusable scroll region (as `.table-container`) as the floor for a card narrower than 326. Never scaled, as Q5 |
| E2 | "Against the minimum then" in the chart's table: the ask's "cannot disagree" is untrue. "n of m" is a stored tally per semester (CreditApplier.cs:234-237); no per-activity outcome is stored; a live read differs after an administrator edits `MinimumLevelByStageJson`, for a credit-None rating, and by window [B4] | Compute the table live with the comparer credit uses (`EntrustmentLevelComparer`, the stage at each encounter), say so on the Spec with both windows named, and drop "cannot disagree". Storing each activity's outcome is a migration for a divergence a rebuild already clears |
| E3 | The EPA page's owner. `NavOwners.OwnerFor` needs the acting role in the entry (NavOwners.cs:184-187), but My progress is offered to every role of a trainee-record holder and to a graduate with no role, so a Trainee-only entry lights nothing for a registrar acting as Assessor or a graduate (trail "Home" alone) [B8] | One rule in OwnerFor: a page owned by a personal link is owned whatever the role, null included. Per-role entries would need one per role and still miss the graduate. `ActiveNavItemTests` reads the page's policy (`TraineeOrFormerTrainee`) |
| E4 | "Open My authorisations" on the EPA page (R2-C-Epa:101) is a dead end for a graduate: My authorisations is `Roles = "Trainee"` (MyAuthorisations.razor:2), the EPA page `TraineeOrFormerTrainee` [B9] | Show the link only to a Trainee; the EPA page's Entrustment section already shows the graduate the STAR. Widening My authorisations to former trainees is T252's pattern and its own task |
| E5 | Which window a decision's line counts. B1 says "the EPA's count now"; C5 says "the activity's own window". A Semester 1 encounter completed in Semester 2 then reads "1 of 3 this semester" on Home and "Semester 1, 2026: 3 of 3, met." on its card [B12] | One rule for both: the activity's own window, named when it is not the current one ("PAED-001, Semester 1, 2026: 3 of 3, met."). The Spec adds the lines the code can produce: a yearly item ("PAED-008: 1 of 1 in 2026"), a waived window (no fraction), credit 0 ("It counted towards no curriculum requirement.", as built), no EPA (no line; "Open My progress" to My progress) |
| E6 | Recent decisions' kinds. B1 names four (Completed, Discussed, Signed off, Declined) under the rule "finished or no move left"; the seeds' other finished states, Accepted, Reviewed, Approved, Verified and Rejected, satisfy the rule too [B17] | The rule, not the list: every finished state, its badge as BadgeFor gives it, "Credits nothing." where it credited nothing; "File it again" stays Declined's alone (as built, ActivityPageModel.cs:293-308) |

## For the build (step 6), not the canvas

These are reads and wiring the boards rightly do not show.
1. **Recent decisions is a new query.** DecidedByYou excludes the subject's own rows (`SubjectUserId != caller`), so the
   trainee's read is not a parameter of it; its names, if they must equal My activities', come from the whole list
   (ListActivitiesBySubjectQuery.cs:117-123): O(all her activities) per Home load, as for the EPA page. [B10, B17]
2. **The paused row line** keys on the credit (CreditedItemCount 0) and the pause, not `EpaInForce == false` alone:
   a completion credited before the pause would read "waits" (ActivityPageModel `DoneStatus`). Not in the cast. [B18]
3. **Home's standing:** GetEntrustmentStandingForTraineeQuery reads every rated activity for latest ratings Home does
   not show; Home needs decisions, the year and exit levels. Fold a summary into the dashboard read, or give the card
   its own error; today one read sits behind DashboardFrame. [B21]
4. **The completed card's count** (C5) is part of the card's own load: it draws once, with the sentence; a failed
   count drops the sentence and leaves flow 03's words, logged. [A13]
5. **The ended view** stays flow 13's (R5); no board draws it, and a graduate's "Open My progress →" lands on it. The
   `no-epa-in-use` page keeps the panel's built empty line, "No EPA on your curriculum is in use at the moment, so there
   is nothing to compare." (EntrustmentStandingPanel.razor:20-23), if the page draws the panel at all. [B22]
6. **The merge into app.css** edits the originals: `.trajectory-chart-dot`, `.is-off-scale` and the grid rule
   (app.css:3424, 3441, 3451) go dead; the lone-chart comment (:2177-2180) goes with R4; the x/y label layering
   replaces :3458. [T6]
7. **Focus moves with no new rule:** "File it again" lands on Log an activity's h1; the crumb back to My progress on its
   h1 (flow 01). Say so in DESIGN.md beside R2. [A14]

## Nits, for the build

KGK-001's cadence cell is a dash (at 390 "Committee decides: —"): omit the label for that row or add a hidden "No
decision cadence" [A11]; the cadence cells use Q4's words ("Decided once a year"), so PAED-003 does not read "Once a
year" inside the "Each semester" group [S17]; the trajectory's heading level is a parameter, h2 on the EPA page, one
below the section's on the committee page [A10]; the stacked trajectory table's Encounter is `th scope="row"` [A9];
furthest short is a `ul` of `li.progress-row`, as its sibling cards [A7]; the chart's 11 px notes ("Today", "Other") at
12 px [T7]; "At it (4, training year 3)" also covers a rating above the minimum, where the panel says "at or above"
[B19]; all-met's "and the 2026 academic year" needs its own sentence for a curriculum with one kind of item [B23]; the
all-met board's decision lines read "3 of 3 this semester, met." under "every target is met" [S14].

## Dropped

- **B16** (du Plessis' Home at A.4.6 holds decisions): the runbook says otherwise. His CBD (3.21) "ends Requested" and
  is still in Dr Khumalo's inbox at 3.28 and 3.51; his portfolio review is R10, "Awaiting review (open)", aged at 3.30.
  3.26's "Patel du Plessis's portfolio review 10 and 14" names the next row after 10 and 14, not a completion. His
  teaching sessions (3.18-3.19) are logged by himself, and nobody else moved them. "No decisions yet." stands.
- **A8** (three "1 item to PAED-001, in My progress" links): the name is C7's, decided; they share a destination.
- **A9's first half** (each rating's link on the EPA page twice): C3 keeps both tables on purpose.
- **B14's trajectory key** ("How to read the chart" and its lines): round 1's Trajectory boards drew the key, and Q5
  kept the trajectory "as round 1 drew it". Its words still go on the Spec's list of new words (C11).
- **Reclassified, not dropped:** S13 (put as a decision) is a correction, C6f having decided "never shortened"; B22,
  T6 and A13 are build notes; S22 and B23's second bullet merge into C8.

## What holds

- Every state at both widths; 112 wrappers, each importing its component at its state and width; each enum equals the
  ask's list; "No replay reaches this" on every hypothetical board. [S]
- Pick, Q1–Q9, O1, O2: Home's four cards, no MSF and no Upcoming deadlines on Home, no move notice, no 11.2 badge, no
  scheduled-review date; the menu unchanged, My progress under the rule, My authorisations with no menu item. [S]
- B1's card (flow 04's classes, "File it again, to someone else" with its accessible name), N1–N6, and C2–C8 but for
  C1–C4 above; F1–F16 (but C1's three values and C6's title) and Molefe's 2.39, 3.48, 4.39, 4.40, 5.15 and 5.21 word for
  word. [S, B]
- The EPA id is the key (one item per EPA per curriculum; an out-of-scope id is not found); the paused sentence is true
  (D48, 6.24); the stepped minimum steps on 2026-01-14 for Molefe and Dlamini; the split reads make `section-error` the
  build; the built words reproduced exactly (PreviousLine, "Target met for", the December Alert, "Your training record",
  `EndedLine`, DashboardFrame's load error). [B]
- Tokens: all 33 colour tokens, radius, shadow, spacing and stacks match tokens.json; no literal colour; every class
  defined; no new name clashes with app.css; every contrast figure recomputes, and every new graphic pair meets 3:1. [T]
- Heading order with no skips; the breadcrumb `nav` with `aria-current`; every card and section labelled by its
  heading; captions and `th scope="row"` on the index; the chart's SVG named for what it adds; the paused mark and the
  KGK badge in text; verdicts in words; load errors `role="alert"` with Try again at 44 px. [A]
