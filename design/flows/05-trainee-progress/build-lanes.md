# T355 step 6 — rules common to every build lane (flow 05, a registrar reads where they stand)

Adapted from `design/flows/04-assessor-inbox/build-lanes.md` (T350). You are one lane of the build of flow 05 of Wombat's
GUI restructure, task T355 (`execution/tasks/in_progress/T355-*.md`). The design is final: Claude Design canvas round 3
(version `1791094162-9f6b`, `round-3-check.md`). The lane plan is at the end of this file; the names two or more lanes
share are in `build-contracts.md`, beside it.

Master is at `1b44bfaf` or later: T304 and T305 are merged, so a curriculum move already replays credit in the same save
and an ended profile is read-only. Flow 05 therefore draws only the rebuilt state after a version move (Q9): no move
notice, no "New in 11.2" mark, nothing that says counts are "being rebuilt".

## Read first (in your worktree)
- `design/flows/05-trainee-progress/README.md` — the design record and every decision (V1 as reviewed, B1, Q1–Q9, O1,
  O2, E1–E6, C1–C13). `design/flows/05-trainee-progress.md` — the brief: the cast, the 17 runbook steps, what is wrong
  today, and § 1's **fence** ("WHAT FLOW 05 MAY CHANGE OF WHAT FLOWS 03 AND 04 BUILT, AND WHAT IT MAY NOT").
- `design/flows/05-trainee-progress/round-3/project/` — the accepted boards. The substance is in the five component
  boards, `R3-C-Home`, `R3-C-Progress`, `R3-C-Epa`, `R3-C-Trajectory` and `R3-C-WaysIn`: each is one component with a
  `state` prop. Read its markup and its `renderVals` script, which holds each state's data and words. The 112 `R3-H-*`,
  `R3-P-*`, `R3-E-*`, `R3-T-*` and `R3-W-*` wrappers only pick a state and a width.
  - `R3-Spec.dc.html` is the contract: every message by kind and its words, and the new words with no source (§ 1), the
    DESIGN.md rules R1–R5 ready to paste (§ 2), the NEW components and their classes, and every 44 px lift (§ 3), the
    contrast figures (§ 4), focus and announcements (§ 5), the reads (§ 6), presses and the measured phone heights
    (§ 7), and the boundaries (§ 8).
  - `R3-Steps.dc.html` lists the runbook Expects that change, and the gaps the design closes.
  - `flow05-r3.css` part 3, "NEW for flow 05", is the canvas's proposal for the new classes. It is a picture, not code
    to copy: rebuild each rule in `app.css` on the spacing scale, extending the built class its comment names. Parts 1
    and 2 (tokens, the canvas frame `.r2-*`) never ship. One leftover: `.standing-summary` (`flow05-r3.css:122`) is used
    by no board; do not ship it.
- `design/flows/05-trainee-progress/round-1-review.md` — the pick (V1, "EPA pages: an index, and a page per EPA", with
  V2's Credited lately widened to recent decisions), corrections 1–6, Q1–Q9, O1, O2, R1–R5.
- `design/flows/05-trainee-progress/round-2-review.md` — corrections C1–C13, decisions E1–E6 (all accepted), and the
  **build notes**: § "For the build (step 6), not the canvas" items 1–7 are **notes 1–7**, and § "Nits, for the build"
  are **notes 8–16** in the order the section gives them (note 8 = A11, 9 = S17, 10 = A10, 11 = A9, 12 = A7, 13 = T7,
  14 = B19, 15 = B23, 16 = S14). Your lane's notes are listed below by number, with the reviewers' evidence (file:line)
  in the review.
- `design/flows/05-trainee-progress/round-3-check.md` — the three "partly" rows and how they are read: C11's
  `[rating] on [scale]` slot is filled by the build from the activity (the rating's label on its own scale); the
  not-started ends line is C2's; the section error's Try again sits under the panel's heading (C8).
- `design/flows/05-trainee-progress/build-contracts.md` — the names you provide or rely on. Use them exactly.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS. CLAUDE.md — conventions and
  footguns, and the Activity platform section (`CreditApplier`, `CurriculumItemsInForce`, D48's pause, T281's end).
- Flow 04's `build-contracts.md` and `build-review.md` — the vocabulary flow 05 builds on: `ActivitySummaryDto`'s row
  fields (`DecidedOn`, `IsFinished`, `DisplayName`), `ActivityLink`, `ActivityRowNames`, `.decided-list`/`.decided-row`/
  `.decided-meta`/`.decided-date`, `.card-empty`, `.only-wide`/`.only-narrow`, `StatePanel.FocusFailureAfterRender`,
  `DecidedByYou` (Recent decisions is its mirror for the subject, R1 of flow 04's fix: a create is never a decision).

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. The repo has no usable remote: nothing is ever pushed.
- Commit in your worktree as you go.
  - Write each message to a file and use `git commit -F <file>`.
  - End every message with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns (the plan below). `app.css` and
  `execution/architecture/DESIGN.md` are shared by section: each lane writes only under its own heading. If you must
  touch a file another lane owns, keep the change minimal and name it in your report. A wave-1 lane that changes a shape
  the Web project reads keeps the Web project compiling with the least change there (as T350's lane A1 kept
  `AssessorDashboard.razor` compiling); wave 2 rewrites the page.

## Never
- Touch the apps running on ports 5080 and 5180, or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any admin credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema, which is expected.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Upgrade MediatR, add Bootstrap/MudBlazor/Radzen/jQuery, put raw hex colours outside `:root`, or use `<i class="bi">`.
- Load anything from a third-party host at runtime (the CSP allows only 'self'); no inline scripts or `onclick`. The
  chart's two drawings are chosen by a container query on `.trajectory-figure`, never by a script and never by the
  viewport (E1); no `?epa=` query and no scroll script (Spec § 5): the EPA page's h1 takes the focus by `FocusOnNavigate`.
- Scale the chart: each drawing has its own fixed size (900 and 326 px), nothing under 11 px, rungs and months 13 px
  (12 px in the 326 drawing); a card narrower than its drawing scrolls it in the named region.
- Write a figure as "n / m", a percentage or a lifetime total, or put the bare word "year" beside a training year and an
  academic year (R3). A waived window shows its count and when targets start, never a fraction or a bar.
- Count or plot multi-source feedback (D8, D36): MSF counts towards no target, a campaign *covers* an EPA (D9), and
  `MsfCoverageText`'s words are kept whole wherever they are placed (C4).
- Change what the fence keeps (brief § 1): the shell and Home's frame; My activities' shape but for its Credit cell;
  `NeedsYouList`'s rows and words; the activity page and its status card's parts, words and order, but for the completed
  card's count and where "Open My progress" lands; "(no longer in use)" as the paused mark's words; flow 04's other-role
  line. Nor the menu (Q8: My progress stays a personal link; My authorisations keeps no item).
- Show a registrar a scheduled review (O1), a move notice or a version mark (Q9), or change KGK-001's periods before it
  was added (O2: today's rule and words).
- Change a seed, a migration or the credit engine: flow 05 reads; it adds no command and no migration.
- Print an exception's text on a page: load errors use the fixed words, and the failure goes to the log (T329, T272).
- Name a test helper `StyleSheet.cs` (it is the same file as `Design/Stylesheet.cs` on Windows).

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids: cite
  T355 and the reviews' ids, e.g. "(T355, note 3)", "(T355, C1)", "(T355, E5)", "(T355, round 1 correction 2)".
- The audit pipeline commits a failed handler's staged rows (`AuditPipelineBehavior`): flow 05 adds reads only, so no
  lane adds a command; if one finds it must, it stops and says so.
- Application handlers go through `IApplicationDbContext`, never EF types. Web components use `IScopedSender` and DTOs,
  never Domain types in `.razor`. A handler that reads `Set<Activity>()` must satisfy `ActivityReadBoundaryTests` and
  `DashboardStateLiteralTests` (no literal state keys); a read about a trainee goes through `WhereReadableBy` or
  `TraineeScopeResolver.MayReadAsync`, and an out-of-scope or unknown id reads as not found (null), never a refusal.
- "Today" is `QuotaCalendar.Today(TimeProvider)` on the South African calendar, never `DateTime.UtcNow` or
  `DateTime.Today` (DESIGN.md; T325). Times shown are SAST with the zone (`ActivityMoments.When`). Dates people read are
  ISO, `2026-11-30`, on every flow-05 surface (T325; decision D1 in `build-contracts.md`).
- Straight apostrophes in app strings; curly ones only in the College's seeded text (C11).
- Change a test deliberately, never loosen it to pass. Keep the Design tests (`tests/Wombat.Web.Tests/Design/*`)
  truthful: `DefinedClassTests` fails a class a page uses that `app.css` does not define, so a class is defined by the
  lane that owns it (`build-contracts.md` § CSS, by owner) and used by the others only after the merge that brings it.
- Each lane adds its classes under its own `/* Flow 05: … (T355) */` heading in `app.css`, and its DESIGN.md rule under
  the section the plan names; the integrator makes the account one after the merge (as T350 did).
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the integrator's
  runbook lane does both after merging. T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. If the new
  route makes it fail, add the route to `coverage.md` § Pages only, and say so (lane D). Name in your report every
  runbook line whose Expect your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release` clean, with no new warnings.
  - Then all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web, Integration
    (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know: the contracts you provide or rely on
  (exact type, member and parameter names, and any way they differ from `build-contracts.md`), files you touched outside
  your list, runbook steps whose Expect your change alters.

## The lane plan

Five lanes in two waves, as T350 ran (`t350-waiting`, `t350-words`; then `t350-lists`, `t350-page`, `t350-form`).
Worktrees under `C:\dev\Wombat\.claude\worktrees\<branch>`. Integration branch **`t355`**, cut from master; each wave's
lanes are cut from `t355` and merged back into it (`Merge lane <X> (<branch>)`), and the whole is squashed into one commit
on master. Nothing is pushed. Ownership is disjoint: a file is in one lane's list (`app.css` and DESIGN.md by section),
and a test file goes with the code it tests unless named.

Wave 1 is the reads and the words, split by what they read: lane A1 the curriculum's counts, one EPA and the trajectory;
lane A2 the decisions on her requests, the count a decision made, the standing's summary and the list filter. Each lane
also writes the Words classes over its own DTOs, so a phrase is tested where its data is built. Wave 2 is the pages:
Home and the ways in (B), My progress with the index and the standing panel (C), and the EPA page with the trajectory
and the owner rule (D).

### Wave 1 (parallel): the reads and the words

**Lane A1 "counts"** (`t355-counts`). The curriculum's counts, one EPA's read, the trajectory's read, and their words.
- Scope:
  - `TraineeCurriculumProgressDto` gains the four fields (round 2, C10): the decision cadence, the opportunistic flag,
    the owning institution's name (KGK-001's "Kgosi Kgari Teaching Hospital's own"), the exit level; and the EPA's
    in-force flag. Init members with defaults, so the portfolio PDF's reader (`PortfolioEpaProgress`) compiles unchanged.
  - The "No longer in use" group (Spec § 6): `TraineeCurriculumProgressSummaryDto.Paused`, the profile's items whose EPA
    is not in force, read but never counted (D48; `CurriculumItemsInForce`'s complement, national or the trainee's own
    institution's only).
  - NEW `GetEpaProgressForTraineeQuery`: one EPA of the caller's own curriculum, by EPA id (round 1, correction 2), in
    force or paused, with its current and previous windows (every period, read-only, for an ended programme, R5), the
    training year, the level for that year, the cadence and the exit level. Null when the caller has no profile or the
    EPA is no item of the curriculum their preferred profile holds (an out-of-scope or unknown id is "not found").
  - The trajectory's read (C10; E2): `GetEpaTrajectoryForTraineeQuery` gains an EPA filter, and its DTOs gain the
    assessor's name, the activity's row name (E7, over the subject's whole list), the training year at each encounter,
    "against the minimum then" computed live with `EntrustmentLevelComparer` at that training year's minimum and the
    item's pinned scale (E2: the same comparer credit uses, `CreditApplier.CompareMinimumLevel`), the stepped minimum
    (the item's per-stage map at `TraineeProfile.StageOn` of each date, in 365-day blocks), the exit level, the other
    scale's label (C11's `[rating] on [scale]` slot, from the activity), and the window read. `TrajectoryWindow`
    gives the academic year containing a day (D40: December counts into it). The committee page's use is unchanged
    in this lane (lane D wires it).
  - `QuotaText` and the progress words read ISO dates (decision D1; T325): `QuotaText.Iso`, and the Web words below use
    it. `MsfCoverageText`'s sentences keep their words and take the ISO date ("closed on 2026-10-03", C4).
  - Web, the words (one class per surface, the built sentences reused, moved out of `MyProgress.razor` word for word
    where the board keeps them):
    - NEW `Shared/Progress/ProgressWords`: figures, count cells, the "more by" line and its December form (C3), waived
      and not-started cells (C2), "Target met for …" with "the" before a yearly window (C3, C11), `PreviousLine`,
      `EndedPeriodLine`, This period's lines (`TargetsLine`, the ends line, the training-year line), the three Alerts
      (December, part-way, not started; C2), the group captions, cadence words (Q4; notes 8, 9), the paused row's N6
      sentence, Home's furthest-short row line.
    - NEW `Shared/Progress/EpaPageWords`: the level line ("Training year 4: level 5, the minimum each encounter is
      judged against and your STAR's target."; KGK-001 "Minimum 3a", C4), "At the minimum level when observed: n of m",
      "Last encounter 2026-09-23", the Entrustment lines (paused, "No STAR yet."), the empties, the page's loading and
      error words, the tab and h1 with the paused mark (C5).
    - NEW `Shared/TrajectoryWords`: the summary under the heading (C11's template; "so far" for one rating only),
      "Multi-source feedback is not plotted.", the region's name, the SVG's name, the key's lines, "No rating yet.", the
      table's "Against the minimum then" cell (note 14: "At or above (5, training year 4)", never "At it" for a rating
      above).
    - NEW `Shared/Progress/ProgressLinks`: the EPA page's address and the accessible names every lane links it by.
    - `EpaLabel` marks a paused EPA with `span.paused-mark` (the words unchanged; C13), so every row, heading and chart
      heading marks it the same way.
  - CSS (`/* Flow 05: the paused mark (T355) */`): `.paused-mark`.
- Files owned: `src/Wombat.Application/Features/Curricula/Quota/TraineeQuotaProgress.cs`, `…/Quota/QuotaProgress.cs`
  (`QuotaText` only), `…/Features/Curricula/GetEpaProgressForTrainee/*` (new), `…/Curricula/Quota/TrajectoryWindow.cs`
  (new), `…/Features/Activities/Queries/GetEpaTrajectoryForTrainee/GetEpaTrajectoryForTraineeQuery.cs`;
  `src/Wombat.Web/Components/Shared/Progress/ProgressWords.cs`, `EpaPageWords.cs`, `ProgressLinks.cs` (new),
  `Shared/TrajectoryWords.cs` (new), `Shared/EpaLabel.razor`, `Shared/MsfCoverageText.cs` (the date only); `app.css`
  (its section).
- Build notes: 8 and 9 (the cadence words), 14 (the table's verdict words), and the words of 5 (the ended periods, R5).
  Round 1: corrections 2 (by id) and 6 (the words that misstated the rules). Round 2: C2, C3, C4 (words), C5 (the tab),
  C10 (the counts' fields, the trajectory's fields), C11 (words), E2.
- Tests: Application — `GetCurriculumProgressForTraineeTests` (the four fields; the paused group lists a paused item and
  counts nothing; KGK-001's owner; another institution's local item never), NEW `GetEpaProgressForTraineeQueryTests`
  (by id; a paused EPA read; an EPA not on the curriculum, another institution's local EPA and an unknown id null; an
  ended programme's periods; the training year and its level), the trajectory's tests (the EPA filter; the window;
  the stepped minimum steps on the day `StageOn` changes, 2026-01-14 for Molefe and Dlamini; "against the minimum then"
  agrees with `CreditApplier` for an on-scale rating at, above and below, and reads NotComparable for another scale;
  the assessor and row names; out of scope reads empty, T101). Web — `ProgressWordsTests`, `EpaPageWordsTests`,
  `TrajectoryWordsTests` (every phrase on the Spec's § 1 for its surface, the built ones byte-equal to today's), the
  `EpaNoLongerInUseMarkTests` mark.
- Provides: every name in `build-contracts.md` § Reads [A1] and § Words [A1]. Consumes: nothing new.

**Lane A2 "decisions"** (`t355-decisions`). What was decided on her requests, the count a decision made, Home's read,
the standing's summary, the list's EPA filter, their words, and the 44 px block links every page uses.
- Scope:
  - NEW `DecidedOnYours`, Recent decisions' read (B1; E6; note 1): the caller's own requests whose last move someone
    else made and that are finished (a terminal state of the pinned workflow) or have no move left; never one whose last
    move is its create, whoever created it (flow 04's R1 fix), and never a system-managed type's record (MSF, "MSF
    records are not listed"); newest decision first; the names by `ActivityRowNames`' rule over the subject's whole
    list (E7); each with its latest credit (T108) and its count line.
  - NEW the count a decision made (C5; E5): `EpaCountLines`, one reader for an activity's EPA and encounter date — the
    item of the trainee's curriculum for that EPA, its window containing the encounter date, tallied by
    `QuotaProgressCalculator` as on today, and whether that window is the current one — shared by Recent decisions and
    the completed card's `GetActivityCountLineQuery` (the caller's own activity only; null for anyone else, for an
    activity about no EPA, or for an EPA that is no in-force item of her curriculum).
  - The standing's reader (note 3): `GetEntrustmentStandingForTraineeQuery`'s body moves into a shared
    `EntrustmentStandingReader`, with a summary mode that skips the latest ratings (Home shows none). The query keeps
    its scope check and calls it; nothing it returns changes.
  - `GetTraineeDashboardSummaryQuery` reshaped (R1): the targets (unchanged), Needs you (unchanged), Recent decisions
    (five), the standing in summary mode (the card's summary and its STAR rows below their level or expiring within 30
    days are the page's to pick from it); Recent activities and Upcoming deadlines retired (Q3; T298). One read behind
    `DashboardFrame`, so a failure is Home's one load error (note 3). Keep `TraineeDashboard.razor` compiling with the
    least change; lane B rewrites it.
  - `ListActivitiesBySubjectQuery` gains an EPA filter (C10): the subject's activities stamped with that EPA in a
    finished state (so a Declined or cancelled request is never listed), MSF rows included, named by E7 over the whole
    list, newest encounter first, as My activities orders. Without the filter it reads exactly as today.
  - Web, the words:
    - NEW `Shared/Progress/CountLineWords` (E5, E6): one rule for a decision's line and the completed card's sentence:
      the current window "PAED-001: 3 of 3 this semester, met.", an older one named "PAED-001, Semester 1, 2026: 3 of 3,
      met.", a yearly item, a waived window, the paused sentence keyed on credit and pause together (note 2), "Credits
      nothing.", no line for no EPA.
    - NEW `Dashboards/TraineeHomeWords`: Home's notices and empties (Spec § 1, Home rows), the STAR row line, the STAR
      card's empty, the File it again accessible name, and all-met's sentence for a curriculum with one kind of item
      (note 15).
    - NEW `Shared/StandingWords`: the standing's summary line, moved word for word out of `EntrustmentStandingPanel`'s
      `YearLine` so Home's card and the panel say it once ("the standing summary in the panel's words", R1). The panel
      keeps its private copy until lane C points it here.
  - CSS (`/* Flow 05: block links at phone width (T355, C7, C9) */`): the base rules of `.progress-row-link`,
    `.epa-link`, `.standing-rating-link`, `.activity-block-link`, `.credit-link`, `.epa-auth-link` and `.decided-note`,
    and the one rule below 641 px that makes each of them, `.decided-row .activity-link` and `.decided-note a` a 44 px
    block whose name and `.activity-link-to` stack (C7: `flex-direction: column`); `.detail-card--empty .btn` at 2.75rem
    below 641 px (C9).
- Files owned: `src/Wombat.Application/Features/Activities/Services/DecidedOnYours.cs` (new),
  `src/Wombat.Application/Features/Curricula/Quota/EpaCountLines.cs` (new),
  `…/Features/Activities/Queries/GetActivityCountLine/*` (new), `…/Activities/Dtos/ActivityDtos.cs` (the init member in
  the contracts), `…/Activities/Queries/ListActivitiesBySubject/ListActivitiesBySubjectQuery.cs`,
  `src/Wombat.Application/Features/EntrustmentDecisions/GetEntrustmentStandingForTrainee.cs` and
  `EntrustmentStandingReader.cs` (new), `src/Wombat.Application/Features/Dashboards/Trainee/*`;
  `src/Wombat.Web/Components/Shared/Progress/CountLineWords.cs`, `Shared/StandingWords.cs`,
  `Pages/Dashboards/TraineeHomeWords.cs` (new); `app.css` (its section). Minimal compile-only edit:
  `Pages/Dashboards/TraineeDashboard.razor`.
- Build notes: 1, 2 (the words), 3, 15, 16. Round 1: the borrowing (V2's Credited lately widened), correction 5 (the
  count names the activity's own window, read live). Round 2: E5, E6, C7 (the block-link rule), C9 (the empty state's
  button), C10 (the activities' filter).
- Tests: Application — NEW `DecidedOnYoursTests` (the rule: a completion, a decline, a discussion, a sign-off, an
  accept by someone else are decisions; a return to her draft is not; her own logged procedure is not; a supervisor's
  create on her behalf is not, even on a type born terminal; MSF is not; newest decision first; five; credit by T108;
  E7 names over the whole list), NEW `EpaCountLinesTests` (the current window; an older window named; a yearly item; a
  waived window; a paused EPA no line; an EPA off her curriculum no line; December's encounter in Semester 2),
  NEW `GetActivityCountLineQueryTests` (her own only; another trainee's activity, an assessor's view and an unknown id
  null), `TraineeDashboardQueryTests` rewritten (the four parts; the standing in summary mode reads no rating),
  `GetEntrustmentStandingForTraineeTests` unchanged and green (the reader is the same code), the list filter's tests
  (a Declined request and a cancelled one never listed; an MSF row listed; another EPA's rows never; paging and E7 as
  today without the filter). Architecture — `ActivityReadBoundaryTests`, `DashboardStateLiteralTests` for the new
  readers. Integration — the Postgres read of `DecidedOnYours` and of the filter, beside `DashboardWaitingPostgresTests`.
  Web — `CountLineWordsTests` (every line of Spec § 1's "A decision's line" and the card's), `TraineeHomeWordsTests`,
  `StandingWordsTests` (byte-equal to the panel's `YearLine` today), a `StylesheetRuleTests` rule for the block-link
  rule's column and its 2.75rem.
- Provides: every name in `build-contracts.md` § Reads [A2] and § Words [A2]; the block-link classes. Consumes: nothing
  new.

After wave 1: the integrator merges A1 and A2 into `t355`, runs the six suites, and corrects `build-contracts.md` to what
was built (its "As built after wave 1" section, as T350 did) before wave 2 starts.

### Wave 2 (parallel): the pages

**Lane B "home"** (`t355-home`). The Trainee's Home, and the ways in: the completed card's count and where "Open My
progress" lands, and My activities' Credit link.
- Scope:
  - `TraineeDashboard.razor` (R1; Q2; Q3): four cards in this order.
    - "Your targets" (`card-targets`, span 2, emphasis, Lucide `target` NEW): the training-year line (T306), the two
      figures in My progress's words, "Furthest short" (a `ul` of `li.progress-row`, note 12): the five furthest short,
      largest shortfall first, then by code, each its own link to its EPA page named "Code — Title", its figure and date
      under the name, never beside it, with its bar (T280); December's form (C3); the notices for all met (note 15),
      started part-way, not started and ended (A.4.6, ISO), the empties for no curriculum and no EPA in use; foot "Open
      My progress".
    - "Needs you": flow 03's, unchanged; its "Open My activities" is Home's one way to My activities.
    - "Recent decisions" (`card-decisions`, span 2): flow 04's `.decided-list` rows over `DecidedOnYours`: the
      `ActivityLink` ("Type · EPA · date", "to <assessor>"), the decision's badge (`BadgeFor.ActivityState`) and its day,
      then `CountLineWords`' line in `.decided-note`; "File it again, to someone else" for Declined alone (its accessible
      name from `TraineeHomeWords`); "No decisions yet."; no footer.
    - "My authorisations" (`card-stars`, `award`): the summary in `StandingWords`' words, the STARs below their level or
      expiring within 30 days, each a link to its EPA page with `TraineeHomeWords.StarRow`; the empties; foot "Open My
      authorisations". The card is no link around its links (T280).
  - Nothing moves the focus on load; the always-present "Loading your Home." is `DashboardFrame`'s, as built (Spec § 5).
    Home's graduate card ("Your training record") and the other-role line are unchanged.
  - The ways in (C5; E5; Q7):
    - `ActivityPageModel.DoneStatus`: after the credit sentence, the count line (`CountLineWords.ForCard`), only when the
      count read answered; paused keyed on credit and pause together (note 2); credit 0 keeps the built "It counted
      towards no curriculum requirement." with no count. "Open My progress" goes to the EPA page
      (`ProgressLinks.Epa`), named "Open My progress at PAED-001", when the count read names an EPA, else to My progress
      itself; its words and place unchanged.
    - `ActivityView.razor` sends `GetActivityCountLineQuery` as part of the page's own load, only for the subject of a
      finished activity; a failure drops the sentence, keeps flow 03's words and the plain link, and is logged (note 4).
    - `ActivityStatus.razor`: the action's accessible name (`ActivityStatusView.ActionAriaLabel`).
    - `MyActivities.razor`: the Credit cell links "1 item" to the EPA page, named "1 item to PAED-001, in My progress"
      (`.credit-link`), when it credited and names an EPA; "—" (MSF) and "None" stay text. Nothing else on the page
      changes (the fence).
  - DESIGN.md: R1 (§ Dashboard page), and in § List page the Credit cell's link.
  - CSS (`/* Flow 05: the Trainee's Home (T355) */`): `.progress-row--link`, `.progress-group-label`, `.targets-line`,
    `.targets-note`, `.stars-summary`; `wwwroot/icons/target.svg` (Lucide).
- Files owned: `src/Wombat.Web/Components/Pages/Dashboards/TraineeDashboard.razor`,
  `Components/Pages/Activities/ActivityView.razor`, `Pages/Activities/MyActivities.razor`,
  `Components/Shared/Activities/ActivityPageModel.cs`, `ActivityStatus.razor`; `wwwroot/icons/target.svg` (new);
  `app.css` (its section); `execution/architecture/DESIGN.md` (R1, the Credit cell); tests under
  `tests/Wombat.Web.Tests/Dashboards/*` for the Trainee's Home (`HomeFrameTests`, `NeedsYouCardTests`, NEW
  `TraineeHomeTests`), `Navigation/DashboardLinkAuthorizationTests.cs`, and the activity page's, status card's and My
  activities' tests under `tests/Wombat.Web.Tests/Activities/`.
- Build notes: 2 (the card), 4, 12, 15 (the page), 16. Round 1: correction 5, the borrowing. Round 2: E5 (the card),
  E6, C3 (Home), C6 (the cast in tests), C8 (Home), C9 (Home's footers, as built).
- Tests: bUnit for every `R3-H-*` state's words and structure (first day, typical, returned, decline, decisions mixed,
  all met, part-way, not started, no curriculum, no EPA in use, ended, December, with STARs, loading, load error,
  two-role, graduate), each row a link of its own and no card a link around them; and every `R3-W-*` state: the card's
  count (current, a later window, an older one), the paused card, the credit-0 card, no EPA (to My progress), a failed
  count (flow 03's words, logged); My activities' Credit link, its name, and "—" and "None" as text.
- Consumes: A2's dashboard summary, `CountLineWords`, `TraineeHomeWords`, `StandingWords`, `GetActivityCountLineQuery`,
  the block-link classes; A1's `ProgressWords`, `ProgressLinks`. Provides: the card ids `card-targets`,
  `card-decisions`, `card-stars`.

**Lane C "progress"** (`t355-progress`). My progress: This period, the EPA index, the paused group, and the standing panel
whole, changed once for My progress and the committee page.
- Scope:
  - `MyProgress.razor` (Q1; R3): header "My progress", subtitle "Training year N · Semester 2, 2026" (2.40); the
    always-present status "Loading My progress." (C8); the page's load error in the fixed words, never the exception's
    text, logged (T329, T272), with Try again answering into This period's heading (`#period-h`, `tabindex="-1"`).
    - "This period" (`section.period-card`): the two figures, the ends line (December's own line), the training-year
      line, MSF's line or its section error ("Multi-source feedback: Could not load MSF coverage. Your counts above are
      not affected."); the December, part-way and not-started Alerts above it (C2, C3).
    - "Your EPAs": NEW `EpaProgressTable` (DataTable `Stack`): one table per group, "Each semester · 10 EPAs" and "Once
      a year · 7 EPAs" captions (`.clinic-table .index-caption`, C7), columns EPA (`th scope="row"`, an `.epa-link` to
      the EPA page named "Code — Title", KGK-001's neutral badge), the window ("Semester 2, 2026" / "2026 academic
      year": the figure, the bar, the meta), "Committee decides" (cadence words; KGK-001 none, with a hidden "No decision
      cadence", notes 8, 9), and "STAR against training year N" (the verdict badge and level, from the standing read by
      EPA id; "Not loaded" when that read failed). Then "No longer in use · 1 EPA" (`tr.is-paused`, EPA and "Why it is
      not counted", N6's sentence), only when a paused item exists.
    - "Entrustment against Annexure A" (`section.standing-panel`, `h2#standing-h tabindex="-1"`): the panel whole, with
      its own section error ("Could not load your standing. …") and Try again under its heading, answering into
      `#standing-h` (C8). Its EPA names link to the EPA page.
    - The trajectories leave My progress (they are on the EPA pages); nothing else on the page draws a chart.
    - The ended view stays flow 13's, as built, but for its dates (D1) (note 5; R5).
  - `EntrustmentStandingPanel.razor` (C1; R4): the built panel, whole, with: `DataTable` `Stack` below 641 px, each
    cell but the EPA labelled by its column; the EPA cell a row header, its name a link by a new `EpaHref` parameter
    (null: plain text, as on the committee page until the integrator wires it after wave 2); the rating link a 44 px `.standing-rating-link`
    ("4 · Encounter 2026-09-24") with `RatingWords` under it in `.standing-rating-meta`; ISO dates ("Issued
    2026-10-03"); "None" for no STAR and no rating; the other ladder's label in place of `[rating] on [scale]` (C11's
    slot); the summary in `StandingWords.YearLine`; the empty line "No EPA on your curriculum is in use at the moment,
    so there is nothing to compare." kept (note 5); its load error in fixed words, never the exception's text.
  - DESIGN.md: R3 (the progress figures rule), and R4's panel half (§ Entrustment standing).
  - CSS (`/* Flow 05: My progress, the index and the standing (T355) */`): `.period-card`, `.period-ends`,
    `.period-lines`, `.period-line-term`, `.section-error`, `.index-section`, `.clinic-table .index-caption`,
    `.clinic-table--index`, `.count-cell`, `.count-figure`, `.count-meta`, `.verdict-cell`, `.verdict-level`,
    `.cadence-none`, `.clinic-table--index tr.is-paused`, `.standing-panel`, `.standing-rating-meta`, and the index's
    and panel's stacked rules below 641 px.
- Files owned: `src/Wombat.Web/Components/Pages/Portfolio/MyProgress.razor`,
  `Components/Shared/Progress/EpaProgressTable.razor` (new), `Components/Shared/EntrustmentStandingPanel.razor`;
  `app.css` (its section); `execution/architecture/DESIGN.md` (R3, R4's panel half); tests
  `tests/Wombat.Web.Tests/Progress/*`, `CommitteeDecisions/EntrustmentStandingPanelTests.cs`,
  `MultiSourceFeedback/MsfCoverageRenderingTests.cs` (the page's half), NEW `Progress/MyProgressTests.cs`,
  `Progress/EpaProgressTableTests.cs`.
- Build notes: 5, 8, 9 (the page). Round 1: correction 1 (the panel whole, under the index), R3, R5. Round 2: C1, C2, C3
  (the index), C6 (PAED-006's title in tests), C7 (the caption), C8 (My progress, `#standing-h`), C13 (`tr.is-paused`).
- Tests: bUnit for every `R3-P-*` state (first, counting, standing, exit met, paused, local item, after move, December,
  part-way, not started, no curriculum, no EPA in use, not comparable, section error, loading, load error); the panel's
  committee view (`Self` false, no `EpaHref`: plain names) and the trainee's (links), stacked cells' labels, ISO dates,
  the exit line's "Not yet: …" in code order, KGK-001's sentences; the standing's Try again focus to `#standing-h`; the
  page's Try again to This period's heading; no exception text anywhere. `StylesheetRuleTests` for `.clinic-table
  .index-caption`'s specificity and the stacked row header.
- Consumes: A1's DTO fields, the paused group, `ProgressWords`, `ProgressLinks`, `EpaLabel`'s mark; A2's `StandingWords`,
  the block-link classes. Provides: `EntrustmentStandingPanel.EpaHref`, the ids `period-h` and `standing-h`.

**Lane D "epa"** (`t355-epa`). The EPA page, the trajectory in its new form on both pages, and the owner rule.
- Scope:
  - NEW `Pages/Portfolio/EpaProgress.razor`, `@page "/portfolio/progress/{EpaId:int}"`, `[Authorize(Policy =
    "TraineeOrFormerTrainee")]` (R2): the h1 "<code> — <title>", "(no longer in use)" after it when paused (C5), KGK-001's
    badge; the tab the h1's words, mark included; the crumb the code. While it loads: h1 "Loading this EPA", crumb
    "Loading…", the always-present status "Loading this EPA."; once loaded the EPA's h1 takes the focus again (C8;
    `ActivityView`'s pattern). Load error: h1 and crumb "EPA", the fixed sentence, Try again answering into the h1.
    Unknown or out-of-scope id: flow 01's not found, word for word.
    - Observations (`#obs-h`): the figure, its bar and meta, the lines (at the minimum when observed, last encounter, the
      window before), MSF's `CardLine` whole (none when paused, C4), the level line; paused: N6's Alert under the h1 and
      no count line; ended: every period read-only with no "n more by" (R5).
    - Entrustment (`#ent-h`): the STAR from the standing read, by EPA id ("Issued 2026-10-03", the exit level and
      whether reached), "No STAR yet.", the paused sentence; "Open My authorisations" (`.epa-auth-link`) only when the
      reader holds the Trainee role (E4).
    - Rating trajectory: `TrajectoryChart` over the EPA's ratings in the academic year containing today (or the one the
      programme ended in), with "Today"; "No rating yet." when none.
    - Activities on this EPA (`#acts-h`): `ListActivitiesBySubjectQuery` with the EPA filter, a `DataTable` `Stack` of
      `ActivityLink`s as 44 px block links (`.activity-block-link`), state and Credit; "No activity on this EPA yet." with
      "Log an activity" (`.detail-card--empty`).
    - Any read failing is the page's load error; the page reads the caller's own record only.
  - `TrajectoryChart.razor` in its new form (Q5; E1; R4; notes 10, 11, 13): both drawings in the markup (900 `--wide`,
    326 `--narrow`), inside `div.trajectory-figure role="region" aria-label="Rating chart for PAED-001" tabindex="0"`,
    chosen by the container query; one window on a real time axis with month ticks (every other month labelled at 326),
    semester 2 banded, a "Today" rule only when given a today, the area below the training year's minimum shaded with a
    stepped dashed edge, "Minimum 5" in text colour (C7), the exit line, the other scale's own lane with hollow points,
    points closer than a dot set a dot apart; the key; the table always visible (Encounter as `th scope="row"`, Rating,
    Against the minimum then, Activity linked by its row name, Assessor; oldest first); the SVG named for what it adds;
    the heading's level a parameter (h2 on the EPA page, one below the section's on the committee page). The points'
    `PointsOf` and the old numeric axis go.
  - `ReviewDetail.razor`, the trajectory section only (decision D2): it draws the new component per EPA over the
    review's period, with no "Today", the trainee's name in the heading's line (4.15), under the section's h3, each
    chart's card `id="trajectory-<EpaId>"`. The standing panel's `EpaHref` (lane C's parameter) cannot compile here
    while C runs in parallel: the integrator wires it after both merge, one line pointing each EPA at
    `#trajectory-<EpaId>` (R4). Nothing else on the committee page changes (flow 07's).
  - `NavOwners` (E3): one rule, a page owned by a personal link is owned whatever the acting role, none included; the
    table gains the EPA page under My progress. `ActiveNavItemTests` reads the page's policy (`TraineeOrFormerTrainee`).
  - `coverage.md` § Pages: the route's row only, if T294's guard asks for it.
  - DESIGN.md: R2 (§ Page-level patterns) with note 7's sentence (no new focus rule: "File it again" lands on Log an
    activity's h1, the crumb on My progress's h1), R4's trajectory half, the lone-chart rule withdrawn, R5.
  - CSS (`/* Flow 05: the EPA page and the trajectory (T355) */`): `.details-grid--reverse`, `.epa-section-title`,
    `.epa-figure-row`, `.epa-lines`, `.epa-level-line`, `.epa-stack`, `.epa-page-alert`, `.star-level`,
    `.activity-cell-paused`, `.trajectory-figure` (and its container query), `.trajectory-card`, `.trajectory-head`,
    every `.trajectory-chart-*` part the Spec's § 3 lists, `.trajectory-key` and its swatches, `.trajectory-table`
    (its caption rule after `app.css:312`, C7), `.trajectory-msf`. Note 6: the originals at `app.css:3418-3460`
    (`.trajectory-chart-dot`, `.is-off-scale`, the grid rule, the label layering) and the lone-chart comment
    (`app.css:2173-2178`) are edited in place, not left dead beside the new rules.
- Files owned: `src/Wombat.Web/Components/Pages/Portfolio/EpaProgress.razor` (new),
  `Components/Shared/TrajectoryChart.razor`, `Components/Pages/CommitteeDecisions/ReviewDetail.razor` (the trajectory
  section and the panel's call only), `src/Wombat.Web/Navigation/NavOwners.cs`; `app.css` (its section and the edited
  originals of note 6); `execution/architecture/DESIGN.md` (R2, R4's trajectory half, R5);
  `execution/knowledge/scenario-paediatrics/coverage.md` (§ Pages, the route's row only); tests
  `tests/Wombat.Web.Tests/Charts/*`, `Navigation/ActiveNavItemTests.cs`, `Navigation/NavMenuAuthorizationTests.cs`,
  `Navigation/PageAccess.cs`, NEW `Portfolio/EpaProgressPageTests.cs`, and the committee page's tests that draw its
  trajectories.
- Build notes: 6, 7, 10, 11, 13. Round 1: corrections 2 (the page by id), 3 (the evidence from the activity records),
  Q5, Q6, Q7 (the landing), R2, R4, R5. Round 2: E1, E3, E4, C4 (the page), C5, C7 (the chart's label and caption), C8
  (the EPA page), C9 (`.epa-auth-link`), C13.
- Tests: bUnit for every `R3-E-*` state (typical, one rating, below STAR, no rating, December, once a year, local item,
  paused, other scale, not found, loading, load error) and every `R3-T-*` (one, three, other scale, paused heading,
  committee): both drawings and the region's name, the table's cells and row header, one point with no line, the other
  scale's lane, the paused heading's mark, no "Today" on the committee page, heading levels; the h1's focus after load;
  "Open My authorisations" only for a Trainee (a graduate reads none); not found for another trainee's or an unknown
  EPA id; `ActiveNavItemTests` (My progress lit for a Trainee, an Assessor acting with a trainee record, and a graduate
  with no role; the trail Home › My progress › the code). `StylesheetRuleTests` for the container query (900 shown at
  ≥ 900 px of container, never by `@media`), the region's focus ring, and nothing under 11 px.
- Consumes: A1's `GetEpaProgressForTraineeQuery`, the trajectory DTOs, `TrajectoryWindow`, `TrajectoryWords`,
  `EpaPageWords`, `ProgressWords`, `ProgressLinks`; A2's `ListActivitiesBySubjectQuery.EpaId`, the block-link classes.
  Provides: the route, `typeof(EpaProgress)` in the owner table, the
  chart's anchor ids `trajectory-<EpaId>`.

### After wave 2 (the integrator, then its own lanes)
- Merge B, C and D into `t355`; wire `ReviewDetail`'s `EpaHref` to `#trajectory-<EpaId>` (one line, with a committee
  page test that the names link to the charts); run the six suites; make
  `app.css`'s five flow-05 sections and DESIGN.md's R1–R5 one account, with the banner naming flow 05 (as `3c239246` and
  T350 did).
- **Runbook-and-docs lane** (`t355-runbook`, as `t350-runbook`), from the built code:
  - The Expects `R3-Steps` lists, in the act files: 2.39, 2.40 (act 2); 3.6, 3.7, 3.12 (the Do too, and the second
    half), 3.48, 3.50 (act 3); 4.15, 4.39 (the Do), 4.40, 4.42 (act 4); 5.8, 5.15, 5.26 (act 5); 6.15 (the Do opens
    PAED-012's row), 6.19, 6.24, 6.27, 6.35, 6.37 (reads as 6.39, F-6.37a closed), 6.39 (act 6); A.4.6, A.7.3, A.7.6
    (the appendix). The routes of 3.7, 5.26, 6.15, 6.24 and 6.27 gain `/portfolio/progress/{EpaId:int}`. Every Expect
    or Actual that quotes a long date this flow now writes ISO (D1) is re-quoted; dated Actual lines are left as written.
  - `coverage.md`: the EPA page in § Pages and § Journeys (what it is reached from), My authorisations' row ("reached
    from Home's card, its STAR rows and the EPA page"), and the gaps closed (F-2.39a/T306, T280 at 3.12, F-A.7.3a/T328,
    F-6.37a/T304, F-A.7.6a/T323).
  - `states.md`: rows for the EPA page's states, My progress's section errors and paused group, Home's four cards' new
    states, the completed card's count.
  - DESIGN.md's banner lists flow 05; `design/flows/05-trainee-progress.md` and any other flow brief that quotes a
    changed Expect follow (as T350's lane did for flows 01, 02, 03, 13, 16 and 19); BRIEF § 8's flow index only if a
    page's flow changes.
- **The build's review**: four read-only reviewers on Sonnet (`review-lanes.md`), the stopping line agreed first;
  `build-review.md`; one fix pass (`t355-fix`, split by back end and pages only if the findings need it); then the squash
  onto master as one T355 commit, with the six suites' counts in its message. Not pushed.
- The integrator records in the squash which task symptoms the build closes (T306's Home half, T298's Upcoming
  deadlines, T346, T280 at 3.12, T328's rating links, T323's chart, T329/T272 on My progress) for the replay (step 7)
  to confirm before any task is closed.
