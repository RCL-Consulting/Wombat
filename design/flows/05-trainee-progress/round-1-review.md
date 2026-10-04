# Flow 05 · Round 1 — the review and the pick

Round 1 was drawn on 2026-10-04 from `round-1-ask.txt` (§ 1 and § 8 as one message). Canvas version `1791076762-5f41`:
16 boards saved in `round-1/project/`. `Main` compares the three structures and gives press counts and the questions'
answers. V1 is "EPA pages: an index, and a page per EPA" (`V1-Home`, `V1-Progress`, `V1-EPA`, `V1-Phone`). V2 is "One
page: every EPA a row that opens in place" (`V2-Home`, `V2-Progress`, `V2-Phone`). V3 is "Weekly Home; My progress in
two parts" (`V3-Home`, `V3-Observations`, `V3-Entrustment`, `V3-Phone`). The shared boards are `Trajectory-1280`,
`Trajectory-390`, `WaysIn` and `Marks`. The canvas makes no pick of its own. The reviewer checked every board against
the code at `39d01a3a` and the runbook's T350 Actual lines (the replay of 2026-09-30 to 10-03, in which every act's `D`
is 2026-10-03).

## The shared parts, common to all three

1. **The trajectory, drawn once** (`Trajectory-1280`, `Trajectory-390`). It is drawn at its own size (900 px; 326 px at
   390), never scaled. It spans one academic year on a real time axis with month ticks, with semester 2 banded and a
   "Today" rule. A shaded area marks what lies below the training year's minimum, stepped where the training year
   changed. The exit level is a heavy line. A rating on another scale is a hollow point in its own lane, "Other scale".
   The table is always visible, with Encounter, Rating, Against the minimum then, Activity (linked, named per row) and
   Assessor. A one-point chart keeps the year's axis and draws no line. The heading names up to three assessors or
   counts them, and "distinct assessors" goes. The smallest text is 11 px at 390 ("Other", "Today"), and the rungs and
   months are 12 px (T323).
2. **The completed status card** (`WaysIn`) adds the count to its second sentence: "Rated 4. Credited 1 item to PAED-001:
   **1 of 3 this semester.**" (T346). "Open My progress" keeps its words and place, and its accessible name becomes
   "Open My progress at PAED-001". Where it lands differs by variation.
3. **My activities** (`WaysIn`) changes one cell. Credit "1 item" becomes a link ("1 item to PAED-001, in My progress")
   to wherever the variation puts the EPA. "—" (MSF) and "None" (paused) stay plain text.
4. **The marks** (`Marks`). A paused EPA has the muted "(no longer in use)" after its name on a row, a card heading and a
   chart heading. KGK-001 has a neutral badge, "Kgosi Kgari Teaching Hospital's own". PAED-016 has "New in curriculum
   11.2" and PAED-011 has "Target changed in 11.2", under a notice: "You moved to curriculum 11.2 on 2026-10-02. Your
   counts carried over. …"
5. **States.** Started part-way: "No target", with no fraction and no bar. December has an info notice. First day,
   every target met and the exit rule met are drawn, as are no curriculum, programme not started and no EPA in use.
   Loading draws the header, then a skeleton. A load error is a sentence under the header with Try again, in My
   activities' words, and a section that fails does so on its own. The other-role line is flow 04's, unchanged. A
   120-character title wraps. A graduate's "Your training record" is today's card, word for word.
6. **One date form** throughout, `2026-11-30`, in place of today's three (T325).

## The three variations

| | V1 · EPA pages | V2 · One page | V3 · Weekly Home |
|---|---|---|---|
| Home (Trainee) | Your targets: training year, "1 of 10" and "0 of 5", the five furthest short as their own links, each "0 of 3 this semester · 3 more by 2026-11-30". Then Needs you (flow 03's), then Your STARs: a summary, the expiring or below rows, and Open My authorisations. Recent activities and Upcoming deadlines go | Due this semester (as V1's targets), Needs you, Coming up (STAR expiry, "a review scheduled for her", the windows' end) and Credited lately (the count each recent completion made) | This week: training year, both figures and one dated Due list merged from three sources, with the short EPAs as chips. Then Needs you and Your standing ("2 of 15", Open Entrustment) |
| My progress | This period (both figures, a third figure for the STARs, then the training year, the exit rule and MSF said once). Then an index table per group (Each semester, Once a year, No longer in use), with columns EPA, count and deadline, Committee decides, STAR against training year N, Exit level. NEW `EpaProgressTable` | This period, then the EPAs grouped by what is due (short this semester, short this academic year, met, no longer in use). Each row is a native `<details>` holding the count, the STAR and the chart. The committee's table sits folded at the foot. NEW `EpaDisclosureList` | Two pages, each its own address, joined by two links: Observations (the counts, with Evidence in a fold per row) and Entrustment (`EntrustmentStandingPanel` moved whole, then every trajectory) |
| One EPA | Its own page, `/portfolio/progress/PAED-010`: Observations, Entrustment, the trajectory with its table as the evidence | The open row | Two places: its Observations row and its Entrustment row and chart, linked both ways |
| "Open My progress" lands on | The EPA page; the h1 takes the focus | `?epa=PAED-001`: the row opens and its summary takes the focus | `?epa=PAED-001`: Observations, the row's fold open, its link focused |
| Menu (Q8) | My progress joins the Trainee's group; My authorisations has no item | As V1 | No move; My authorisations folds into Entrustment |
| Version move (Q9) | All three: only the rebuilt state, plus the notice | | |
| Phone, My progress (estimate) | ≈ 3,600 px | ≈ 2,400 px with every row shut | ≈ 3,000 + 3,400 px |

## Reading them against the code and the brief

- **All three stay inside the fence, apart from three things.**
  - **The completed card's other sentences** (`WaysIn`'s note). The paused one becomes "Credited 1 item to PAED-012,
    which is no longer in use; it counts again if the College restores it." The built words are "Its credit to PAED-012
    waits while the EPA is paused." (`ActivityPageModel.cs:355–356`, Step 6.18's Actual). A completion in the pause
    credits nothing (`CreditedItemCount` 0), so "Credited 1 item" would be false. The MSF one becomes "Recorded.
    Multi-source feedback counts towards no target.", where today's card takes the `CreditsNothing` path. The brief lets
    flow 05 add the count. It keeps the card's other parts and words as flow 03 built them.
  - **My activities' MSF rows.** `WaysIn` gives them a second line, "campaign closed". Flow 03's rows have no such
    line, and only the Credit cell may change.
  - **Q8** (V1, V2) changes flow 01's shell rule. That is the operator's call, below.
  - **Everything else holds.** Each variation keeps Home's frame and Needs you as built. Each says which of flow 03's
    pieces it changes (Main, "What each variation changes").
- **V1's index replaces the committee's table on My progress, which T166 forbids.** V1-Progress's note says its STAR
  and Exit cells are `EntrustmentStandingPanel`'s own cells, while "the committee page keeps its panel". The result is
  two tables. Step 4.40 expects "The page reads the same table as the committee's (T166)", and the brief asks for the
  panel to be changed once, for both pages. V1 also drops the panel's Latest rating column and its opening sentence. V2
  and V3 keep the panel whole. The fix keeps V1's structure: the panel stays whole on My progress, under the index, and
  the index keeps at most a verdict column read from the same `EntrustmentStandingDto` (`EpaStandingDto.YearStatus`).
- **All three remove the Home surface where a registrar learns of a decline.** No mail announces a decline (T320).
  Needs you leaves it out by T297's rule, because a declined request has no move left. Today the registrar finds it
  on Recent activities (the comment at `GetTraineeDashboardSummaryQuery.cs:62–64`). Step 3.12's route is
  "Open the declined Mini-CEX from the dashboard's Recent activities". V1 and V3 drop that card. V2's Credited lately
  lists only completions that credited. In all three, a decline then shows only on My activities. The brief's "may"
  list lets flow 05 change Recent activities, but not this way without a replacement. The fix is to borrow V2's card and
  widen it to what was decided on her requests lately: completed (with the count it made) and declined (with "File it
  again"). It would be newest decision first, five rows, each named per row. Step 3.12 then needs rewriting with the
  flow's steps.
- **V2's weight and order.** Every row's body is rendered on the server up front: 15–17 charts and tables in one circuit
  render tree. Rows also move between groups as she logs. Its chart sits in a 280 px column at 1280 too, so the narrow
  form is the only form most readers see. The committee's table is folded at the foot, so standing at 390 takes two
  presses.
- **V3 keeps the most of today's build, but answers question 1 with "two places".** Folding My authorisations into
  Entrustment does more than move a page. Step 4.39 downloads certificates there, and the graduate's lost certificates
  are T311's (flow 13). The owner table's row "My authorisations | Trainee: My progress" (DESIGN.md § The NavMenu)
  changes as well.
- **What V1 needs that does not exist yet:**
  - **The EPA page:** a new route, owned by My progress (a new owner-table row).
  - **Addressing by id, not code.** An EPA code is unique only within its namespace, national or one institution's
    (`TraineeCurriculumProgressDto.EpaId`'s comment). Address the page by EPA or curriculum item id, not by
    `/portfolio/progress/PAED-010`, and use the same id for "Open My progress" and the Credit link.
  - **Paused items.** The "No longer in use" group needs a read of the profile's items that are not in force.
    `TraineeQuotaProgressReader` reads `.InForce()` only, and the standing does the same
    (`GetEntrustmentStandingForTrainee.cs:119`).
  - **The STAR summary on Home** needs the standing read, which the dashboard does not make today.
- **The evidence is not the trajectory.** V1's EPA page uses the chart's table as "every activity that rated this EPA",
  which is what `AttributedRatings` returns. Two things are missing from that.
  - **Credited activities with no rating.** Any type with a `counts_for` directive and no rated field would count but
    never be listed. Today every crediting type in the corpus is rated (`reflective_exercise_cpsa` and
    `portfolio_review_cpsa` credit nothing; `kgk_teaching_log` declares `{"counts_for": []}`, Step 1.27), so the corpus
    hides the gap.
  - **No per-activity credit link.** Nothing stores which item a completion credited. There is
    `ActivityTransition.CreditedItemCount` and the per-semester tallies. A per-EPA evidence list is a new read: the
    trainee's finished activities stamped with the EPA, with their credit outcome. It goes through T343's query, as the
    brief warns.
- **Invented, or not derivable from what the server holds:**
  - **"Your counts carried over"** after a move. That is T304's open defect (P2, queued). Today every card reads 0 until
    the Administrator's rebuild (Step 6.37's Actual).
  - **"You moved to curriculum 11.2 on 2026-10-02".** `TraineeProfile` stores neither the date of a move nor the version
    it left. The audit log alone records the save. In the replay the move was on 2026-10-03 (Step 6.36).
  - **"New in curriculum 11.2" and "Target changed in 11.2".** Both need the version she left, which is not stored. No
    link joins two curriculum versions either: `Curriculum` has no predecessor.
  - **"Not on your curriculum then"** for KGK-001's 2025 line (`Marks`, posed as a question). `CurriculumItem` has no
    date it was added, so the closed period cannot be told apart. Today's "0 of 1, 1 short" is the rule the items page
    warns of (Step 6.27).
  - **"A review scheduled for her"** (V2's Coming up, V3's Due). The date exists (`CommitteeReview.ScheduledOn`), but a
    trainee is never shown a review before it is ratified: `ListReviewsForTraineeQuery` returns Ratified, Under appeal
    and Final only. The row's link to My committee reviews would open a list that does not hold it. In the replay a
    review is scheduled and sits on the same `D` (Step 4.6), so the row never fills. Showing it is a new disclosure.
    That is the operator's call, and it matters only if V2's or V3's list is borrowed.
  - **The December notice** reads "Yearly targets start again on 2027-01-15". The year starts on 1 January, and 15
    January is the cast's programme start. Today's notice also says December counts towards "your 2026 yearly targets"
    (`MyProgress.razor:86–89`), and the board drops that. In December the cards' "3 more by 2026-11-30" becomes "3 more;
    encounters in December still count towards Semester 2, 2026" (`:467–471`). Home's rows and the EPA page must do
    the same.
  - **"Programme not started"** says "You can log activities before then; they count from it." A date before the
    programme start is refused for any type that can credit (Step 3.9, `EncounterDateGate`). The board flags this
    itself.
  - **Step 4.40's** "Encounter 2026-09-24 · Direct observation; below the target" is PAED-001's latest rating: Dr
    Patel's DOPS at 4, whose evidence category is Direct observation. The boards copied it onto PAED-010.
- **The three "per"s.** The boards' words are true to `QuotaProgressCalculator` and the curriculum items, with five
  slips.
  - **Observation targets.** "1 of 3 this semester" and "0 of 1 in the 2026 academic year" name the window `QuotaWindow`
    tallies. December counting into semester 2 and into the year (D40) is the notice's job. Dropping "(6 a year)" from
    the target line is right: it put the bare word "year" beside "training year".
  - **Decision cadence.** "Decided each semester" or "once a year" comes from `CurriculumItem.DecisionCadence`, and is
    correct per EPA (PAED-003, 006, 007 and 015 have a target each semester and a decision once a year). The first slip:
    PAED-008, 009 and 013 are decided "as opportunity allows" (`DecisionIsOpportunistic`, Act 4's starting state),
    which V1-Progress's "all decided once a year" overwrites. The second: KGK-001 has **no decision cadence** (Step
    6.26), where `Marks` says "decided once a year".
  - **Level targets.** "Training year 4's target: 5" is the item's per-stage map at `GetStage(today)`. The same number
    is the encounter's "Minimum now". V1-EPA says it twice under two names ("Minimum now 5 (training year 4)" and
    "Training year 4's target is 5"). Round 2 should say it once per EPA, as the level both the encounter minimum and
    the STAR target are read against.
  - **Placeholders where the seed has the values.** The yearly targets are 1 (`[n]` on every board). The year-4 targets
    are 5 for PAED-001–007, 010 and 012, and 4 for PAED-008, 009, 011, 013, 014 and 015. The exit level is 5 for nine
    EPAs and 4 for six. KGK-001's minimum is 3a, with no per-year map.
  - **Group counts.** The 17-card state is 10 each semester and 7 once a year (Step 6.37's Actual), not "11 and 6".
- **The trajectory is buildable from what the server knows,** with one rule to share.
  - **What is already there.** Points carry `ActivityId`, `AssessorUserId`, `Source`, `OffLadder` and, through
    `AttributedRatings`, the rated scale.
  - **Derivable:** the stepped minimum (the item's per-stage map at `GetStage` of each date, in 365-day blocks, hence 14
    January), the exit line, the months and the semester bands, the assessor's name (a user lookup) and the activity's
    row name (a type-name join).
  - **The lane is new.** Today an off-ladder point is drawn hollow at its ordinal height (`TrajectoryChart.razor:42`).
  - **The shared rule.** "Against the minimum then" must use the same rule as `CreditTargetResolver`'s
    minimum-level-reached, so the table and "At the minimum level when observed: n of m" cannot disagree.
  - **Flow 07.** `ReviewDetail` draws the same component through `PointsOf`.
    - **The axis.** It should span the review's evidence window, and the "Today" rule should not be drawn on a past
      review.
    - **The tables.** Visible tables lengthen the committee page by one table per rated EPA.
    - **The links.** Activity links are fine: the activity page admits a reader in scope.
    - **DESIGN.md.** The rule that a lone chart scales with its card (§ around line 2777) changes.
- **`EntrustmentStandingPanel` changes reach flow 07 as well.** These are: stacking below 641 px, 44 px rating links
  (T328), ISO dates in place of `QuotaText.ShortDate`, and the EPA name linking down to its chart. The last needs a
  parameter, so that on the committee page it targets that page's chart.

## The cast and the dates

Every state a step names must carry that step's rows. The boards mix steps (Dr Dlamini's "1 of 10" from Step 3.50
beside her PAED-001 "1 of 3" from Step 3.7), and they invent evidence.

- **Dr Molefe's PAED-010** (V1-EPA, V2-Progress, both trajectory boards) has one rating: Dr Zulu's Direct Observation,
  2026-09-29, at rung 5, at the minimum (R17). The boards give three ratings, all invented:
  - "Mini-CEX · PAED-010 · 2026-03-12, Fatima Khumalo, 3b" and "2026-05-20, Mohammed Patel, 4" are invented and
    impossible. PAED-010's tools are Direct observation and MSF only, so the Mini-CEX picker does not offer it (Step
    3.25), and Act 3 files the first record.
  - "Direct Observation · 2026-09-24, David Naidoo, 4" is invented too.
  - The page's other figures follow from the one real rating: "At the minimum when observed: 1 of 1" (not 0 of 1), "Last
    encounter 2026-09-29", "Semester 1, 2026: 0 of 3, 3 short" (not 2 of 3), and "at the minimum" (not "All below").
- **The MSF campaign** closed on 2026-10-03 (Step 3.48's Actual), not 2026-09-30.
- **Dr Molefe's Credited lately** (V2-Home) and **her My activities rows** (`WaysIn`) are invented. Newest encounter
  first, her completions are:
  - Direct Observation PAED-010, 2026-09-29, Thandi Zulu;
  - CBD PAED-012, 09-28, Fatima Khumalo;
  - Mini-CEX PAED-012, 09-26, Sarah Botha;
  - DOPS PAED-001, 09-24, Mohammed Patel;
  - CBD PAED-001, 09-22, David Naidoo;
  - Mini-CEX PAED-001, 09-21, Thandi Zulu.

  The two MSF rows are dated 2026-10-03.
- **Dr Molefe's latest ratings** (V3-Entrustment):
  - PAED-001: "4 · Encounter 2026-09-24", Direct observation, below the target (the DOPS), not "5 · 2026-09-26 Mini-CEX".
  - PAED-010: "5 · 2026-09-29".
  - PAED-012: "5 · 2026-09-28", not "—".
- **Dr Dlamini's paused PAED-012** has one rating: Dr Patel's Mini-CEX, 2026-10-03, at 4 (Steps 6.16–6.19). The boards
  have "2 ratings, from Fatima Khumalo and David Naidoo" and a My activities row "2026-09-28, to Fatima Khumalo".
- **No paused EPA beside its STAR.** V3-Entrustment draws PAED-012 in Dr Molefe's standing at 5 and its chart "(no
  longer in use)" on the same page. During the pause the standing lists in-force items only, and Dr Molefe has graduated
  by Act 6.
- **Dr Dlamini has no PAED-015 evidence.** V1-Phone draws her PAED-015 at "1 of 1 this semester · met". That would make
  her "2 of 10".
- **The off-scale example** (PAED-005, "Mini-CEX · 2026-09-12, Mohammed Patel"; "[Instrument], Sarah Botha") is drawn
  on Dr Dlamini, who has nothing on PAED-005. A Mini-CEX cannot be filed on PAED-005 (its tools are CBD, DOPS and MSF).
  The state is one no replay reaches, so draw it on a placeholder registrar and a permitted instrument.
- **Smaller slips:**
  - The completed card's time is "2026-10-03 17:15 SAST" (Step 3.6's Actual), not "2026-09-24 08:12".
  - The version move was on 2026-10-03.
  - The 17 cards split 10 and 7.
  - Real titles are known for KGK-001 ("Running a paediatric outreach clinic at a district hospital") and PAED-016
    ("Transitioning adolescents with long-term conditions to adult care"), which is once a year, target 1.
  - "Training year 4 on 2026-10-04" should be the replay's `D`.
  - V3-Observations heads Once a year "5 EPAs" while it holds KGK-001, which makes 6.
- **Hypothetical, so fine:** the other-role line's registrar who also acts as Assessor (no registrar in the cast holds
  Assessor, README § Cast), started part-way (`D42`: every start is 15 January), no curriculum, and December.

## The press counts, re-counted

| Journey | V1 1280 / 390 | V2 1280 / 390 | V3 1280 / 390 | Note |
|---|---|---|---|---|
| The weekly look | 0 / 0 | 0 / 0 | 0 / 0 | Holds. Beyond the five furthest short, every variation takes 1 |
| The evidence behind one short EPA | 1 / 1 | 1 / 1 | 2 / 2 | V3's 2 is a choice: its chips could carry `?epa` and open the fold, as "Open My progress" does, for 1 |
| From a completed Mini-CEX to its EPA's count | 0 to read it (the card), 1 to the EPA | as V1 | as V1 | Holds, but counted from the activity page. Main heads the table "Presses from Home". From Home it is 3 in V1 and V3 (My activities, the row, Open My progress) or 2 through the new Credit link. In V2 it is 0, since Credited lately says the count |
| Standing against Annexure A | 1 / 1 | 1 / 2 | 1 / 1 | Holds. V1's 1 is to its own table (T166, above). With the panel kept whole under the index it is still 1, further down at 390 |
| One EPA's trajectory beside its STAR | 2 / 2 | 2 / 2 | 1 / 1 claimed | V1 is 1 if Home's STAR row links to the EPA page. In V3 the chart sits below all 15 table rows, not beside its STAR: 2 by the EPA name's link down, or 1 and a long scroll |
| A paused EPA | 1 / 1 | 1 / 1 | 2 / 2 | Holds |
| KGK-001 | 1 | 1 | 1 | Holds |
| What the move to 11.2 did | 1 | 1 | 1 | Counts a notice that cannot be built as drawn (Q9) |

The phone heights are the canvas's estimates. They are plausible from the stated row heights (V1 about 230 px a row),
but nobody has measured them.

## Accessibility at the wireframe level

- **Focus on arrival.**
  - V1's EPA page needs nothing new: `FocusOnNavigate` already focuses the h1.
  - V2's and V3's `?epa` landing needs `PageFocus`/`wombat.focusById`, which exists (flow 03).
  - A V2 `<summary>` may hold no link and no heading element. Its accessible name is the EPA and the count.
  - At 390, the fold's name "Entrustment against Annexure A · Show" has a stray "Show".
- **Targets.**
  - Home's rows at 390 are 44 px blocks. V1's stacked index link and the EPA page's activity links at 390 have no
    minimum height (`V1-Phone`'s `.srow a`). Give them 44 px, as `Trajectory-390` does.
  - V3's chips are 44 px. Its "4 more" chip needs a name ("4 more short EPAs this semester").
- **Names.**
  - V3-Observations' "Trajectory" and "Evidence · 1 activity" links repeat on every row at 1280. Name them per EPA, as
    the 390 board does (T280).
  - Home's rows are named for their EPA, and the count sits outside the link. Good.
  - The Credit link's name, "1 item to PAED-001, in My progress", is good.
- **The chart.**
  - The SVG is `role="img"`, labelled by the section's own h2, so it repeats the heading. With the table visible and
    adjacent, the SVG can be `aria-hidden`, or named for what it adds ("Chart of the 3 ratings below").
  - The shaded region and the hollow points are not colour alone: the table says "Below (5)" and "another scale" in
    words. Good.
- **Structure.**
  - Index tables need captions and row headers (`th scope="row"` on the EPA).
  - V3's two links are a `nav` with `aria-current`, not a tablist. Good.
  - The load error, Try again and its focus follow My activities (`StatePanel`, `FocusAfterRetry`). Good.
- **Progress bars** keep "n of m" in words beside them on every board (T130's rule).

## Questions 8 and 9, the operator's

**Q8: does My progress (and My authorisations) move into the Trainee's group?** The options the boards give:

- **(a) V1/V2:** My progress moves into the Trainee's group, after My activities. My authorisations keeps no menu item
  and is reached from Home's STAR card, each STAR cell and the EPA page.
- **(b) V3:** no move. My authorisations folds into Entrustment.

DESIGN.md § The NavMenu makes My progress personal so that it is offered whatever the acting role: "A graduate acting
as Assessor sees it", as does a registrar acting as Assessor. Moving it into the Trainee group therefore makes its place
conditional: in the group when acting as Trainee, under the rule otherwise and for a former trainee. That has to hold
"No page twice" and pass `NavMenuAuthorizationTests` and `ActiveNavItemTests`. The menu keeps 8 links either way
(`FlatLimit`). **Recommendation: no move, and no item for My authorisations.** My progress is always in view under the
rule, and Home now leads into it by each EPA. The move would buy one row's position at the cost of a conditional rule
in the shell. My authorisations stays its own page under My progress (owner table unchanged), linked from Home, the EPA
page and each STAR. **The operator's.**

**Q9: after a move to a new version, a notice that her counts are being rebuilt, or only the rebuilt state?** The boards
draw only the rebuilt state, plus the notice "You moved to curriculum 11.2 on … Your counts carried over. New: PAED-016.
Changed: PAED-011", and offer a fallback notice if the rebuild stays apart. **Recommendation: the rebuilt state only,
with T304 landed before or with flow 05's build, and no move notice.**

- **"Carried over" is T304's.** It is true only once T304 lands.
- **The notice needs a stored move.** Its date and the version left are not stored, so it, "New in 11.2" and "Target
  changed in 11.2" need T304 to record the move. The operator may want that. If so, add it to T304's scope and keep the
  notice for the period of the move.
- **If T304 cannot land first.** The page can tell that a rebuild is due: the trainee holds tallies on items outside the
  profile's curriculum, the "stale tallies" the rebuild removes (Step 6.38). A truthful notice is then "Your evidence
  from curriculum 11.1 is not counted here yet. An administrator's rebuild counts it." "Being carried over" is not
  truthful, because nothing is running.

**Other points that are the operator's:**

- **Showing a registrar her review's scheduled date.** Today she sees a review only once it is ratified. This matters
  only if a Coming up row is borrowed. Recommendation: not in flow 05.
- **KGK-001's closed periods.** Keep today's "0 of 1, 1 short" in a period before the item existed, or record the day
  an institution's item was added (new data, a task). Recommendation: keep it in flow 05 and file a task if wanted.

## Recommendation

**V1, "EPA pages: an index, and a page per EPA", with one borrowing from V2.**

- **Why V1.** It answers question 1 with one place per EPA: the count, the STAR, the chart and the evidence on a page
  with a real address. "Open My progress", Home's rows and My activities' Credit cell can each land there, and the h1
  takes the focus with no script. Its index puts the shortfall and the standing side by side at 1280 and stacks at 390.
  It keeps Home short (three cards) and gives T298 real dates without a card of its own. It also keeps the committee's
  table usable once the correction below restores it.
- **Against V2.** V2 renders every EPA's body into one circuit, moves rows as she logs, and folds the standing.
- **Against V3.** V3 leaves each EPA in two places, and builds its Home round a merged list with a source the trainee
  cannot see.
- **The borrowing.** Take V2's Credited lately, widened to what was decided on her requests lately (completed, with the
  count it made, and declined), so that Home keeps the place a decline is found (Step 3.12).

**Corrections to V1:**

1. **Keep `EntrustmentStandingPanel` whole on My progress, under the index** (T166, Step 4.40, flow 07). The index keeps
   one verdict column read from the same standing DTO, or none. The panel's fixes are made once, for both pages:
   stacking below 641 px, 44 px rating links, ISO dates, and the EPA name linking to its EPA page when `Self`.
2. **Address the EPA page by id** (EPA or curriculum item), not by code. Use the same id in "Open My progress", Home's
   rows and the Credit link. Add a "My progress" row to the owner table.
3. **The evidence is the activities on that EPA, read from the activity records.** That means the rated, the credited
   and the MSF that covers it, each with its credit outcome. The trajectory's table is not a substitute.
4. **The version move draws only the rebuilt state** (Q9). Drop "carried over", the move date and the two 11.2 badges
   unless T304 records the move.
5. **The completed card changes only by its count**, and the count names the activity's own window: "Semester 1, 2026:
   3 of 3, met" for an older encounter. It is read live, so the words say the count now ("PAED-001: 2 of 3 this
   semester"), not the count this activity made. The paused and MSF sentences stay flow 03's. My activities' rows
   change only the Credit cell.
6. **Fix the words that misstate the rules:**
   - the December notice, and the December form of "n more by";
   - "programme not started" (a pre-start date is refused);
   - KGK-001's decision cadence (none);
   - PAED-008, 009 and 013 "as opportunity allows";
   - "Minimum now" and the year's STAR target, said once per EPA;
   - MSF in `MsfCoverageText`'s words at 390 too. V2-Phone and V3-Phone shorten them.

**Watch in round 2:**

- **The cast.** Every state carries its step's rows (above): Dr Molefe's six WBAs and two MSF records as Act 3 files
  them, Dr Dlamini's paused PAED-012, and the seed's real targets, levels and titles in place of `[n]`.
- **States not yet drawn on V1.**
  - The EPA page states the board only lists: no evidence, target met, paused, KGK-001 and once a year.
  - The index with 17 rows, and with 15 STARs all at exit.
  - The withdrawn registrar who keeps the Trainee role: Home's targets card reads "Your programme ended on …" (Step
    A.4.6). That state is flow 13's, but V1's card must keep it.
  - "Not comparable" in the panel.
  - The Home decisions card with a decline.
  - Loading and error on the EPA page.
- **The trajectory on the committee page:** the review's window as the axis, no "Today", and the trainee's name in the
  second line. Agree its place with flow 07.
- **Wording:** one name for the STAR card and its page ("Your STARs" beside "Open My authorisations"), and "this
  semester" where the header already names Semester 2, 2026.

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| Pick | Which structure? | V1, with V2's Credited lately widened to recent decisions (completed with its count, declined), and corrections 1–6 |
| Q1 | What leads My progress; one place per EPA? | This period, then the EPA index (count, deadline, cadence, a STAR verdict), then the committee's panel whole. One place per EPA: its own page, addressed by id |
| Q2 | Home against My progress | Home: training year, both figures, the five furthest short (each a link to its EPA page, with its date), Needs you, recent decisions, Your STARs (the summary and the expiring or below rows). No MSF on Home. Recent activities becomes recent decisions |
| Q3 | Upcoming deadlines | Removed. Its real dates live on the rows: the window's end on each short EPA, and the STAR expiry on Your STARs (T298) |
| Q4 | The three "per"s | As the boards: every figure names its window; cadence is its own label ("Decided each semester", "once a year", "as opportunity allows", or none); the level names the training year, said once per EPA |
| Q5 | The trajectory | As drawn: its own size at each width, a time axis, the stepped minimum, the exit line, an other-scale lane, a visible table with assessor and activity. One point: the year's axis, no line. On the committee page, the review's window and no "Today" |
| Q6 | The marks | Paused: "(no longer in use)" after the name on row, heading and chart. KGK-001: a neutral badge. PAED-016: no mark unless T304 records the move (Q9) |
| Q7 | Where "Open My progress" lands | The EPA's page, whose h1 takes the focus. The card adds the count for the activity's window, read live |
| Q8 | Menu (**the operator's**) | No move: My progress stays a personal link; My authorisations keeps no item and is linked from Home, the EPA page and each STAR |
| Q9 | Version move (**the operator's**) | Only the rebuilt state; T304 lands before or with the build; no move notice unless T304 records the move |
| O1 | A registrar's scheduled review date (**the operator's**) | Not shown in flow 05 (today's read shows ratified reviews only) |
| O2 | KGK-001's periods before it existed (**the operator's**) | Keep today's rule and words in flow 05; a task if the add date should be recorded |
| R1 | DESIGN.md § Dashboard page | The Trainee's Home is three cards plus recent decisions; a card's rows are its links, never one link around them |
| R2 | DESIGN.md § Page-level patterns | New: a record page under a personal link (Home › My progress › the EPA), owned by My progress |
| R3 | DESIGN.md's progress-figures rule | Kept and extended: cadence and level name their own frame; a waived window shows its count and when targets start, never a fraction or bar |
| R4 | DESIGN.md § Entrustment standing, and the trajectory | Changed once for My progress and the committee page: stacked below 641 px, 44 px links, ISO dates; the chart at its own size with its table visible |
| R5 | "My progress once the programme has ended" (flow 13) | Holds: the ended view keeps every period read-only; the EPA page shows that EPA's periods the same way |
