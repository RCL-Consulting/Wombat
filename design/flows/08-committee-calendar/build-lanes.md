# T364 step 6: rules common to every build lane (flow 08, the committee calendar)

Adapted from `design/flows/07-review-sitting/build-lanes.md` (T361). You are one lane of the build of flow 08 of Wombat's
GUI restructure, task T364 (`execution/tasks/in_progress/T364-*.md`). The design is final: Claude Design canvas round 3
(version `1791874392-bd1c`, `round-3-check.md`). The lane plan is at the end of this file. The names two or more lanes
share are in `build-contracts.md`, beside it.

## Read first (in your worktree)
- `design/flows/08-committee-calendar.md`: the brief. It holds the cast, the pages table, § 5's known problems with
  file:line evidence, and the 30 runbook steps in § 8.
- `design/flows/08-committee-calendar/round-2-ask.txt`: the pick (A: two pages, the schedule a form page of its own at
  `/committee/reviews/new`), Q1–Q10 and C1–C14. Q7 is **reversed** by F1.
- `design/flows/08-committee-calendar/round-2-review.md`: M1–M10 and the should-fix list. Its "Settled from the evidence"
  list includes the seed's cadence and **"Data the build must add"**.
- `design/flows/08-committee-calendar/round-3-ask.txt`: F1–F6, the cadence, M1–M10 and S1–S8.
- `design/flows/08-committee-calendar/round-3-check.md`: what the build carries. In particular:
  - **F4's default is the code's** (`CommitteeReviewTypes.DefaultFor`): Annual progression for a general panel in both
    semesters. The S1 boards draw Entrustment-only, which is wrong.
  - NothingToDecide's sentence is the handler's own.
  - EPA titles come from the data, never from a board.
- `design/flows/08-committee-calendar/round-3/project/`: the accepted boards (the files keep their `R2-` names).
  `round-3/boards-text.md` is their text.
  - `R2-0-spec.dc.html` is the contract. It holds round 3 item by item, the components, the NEW classes, the DESIGN.md
    changes, the contrast pairs, the step counts, "Data the build must add" and the Flags.
  - `R2-0-steps.dc.html` lists every runbook Route, Do and Expect that changes.
  - The boards' `<helmet>` CSS is the canvas's picture, not code to copy. Rebuild each rule in `app.css` on the spacing
    scale. The shared classes are already there (`build-contracts.md` § 0).
  - A board caption is never a shipped word. A bracketed or placeholder figure is read from the data.
- `execution/knowledge/EPA-PROGRAMME.md`: D40 (the semester labels), D42 (a start inside a semester's first month
  counts), D53 (no deferral).
- `execution/architecture/DESIGN.md`: mandatory before touching any Razor or CSS. Also read CLAUDE.md: its conventions
  and footguns, § Roles, and the InstitutionalAdmin scope rules (a null or not-found for an out-of-scope id, never a
  refusal that leaks it).

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. No lane pushes, and the integrator does not either.
- Commit in your worktree as you go. Write each message to a file and use `git commit -F <file>`. End every message with
  the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns.
  - `app.css`, DESIGN.md and `CommitteeDecisionDtos.cs` are shared by section: each lane writes only under its own
    heading, or adds only its own records.
  - If you must touch a file another lane owns, keep the change minimal and name it in your report.

## Never
- Touch the apps on ports 5080 and 5180 or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves, and each works in its own throwaway `it_<guid>` schema.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Add a migration. Flow 08 needs none. If you find you need one, stop and say so in your report.
- Upgrade MediatR; add Bootstrap, MudBlazor, Radzen or jQuery.
- Put any of these in shipped markup or CSS:
  - raw hex colours outside `:root`;
  - an inline `style=`;
  - `<i class="bi">`;
  - anything loaded from a third-party host;
  - an inline script or `onclick`.
- Use the native `disabled` attribute on an action the design says is unavailable. Use `aria-disabled="true"` with
  `.is-unavailable`, and put the reason beside it, named by `aria-describedby` (DESIGN.md).
  - The one exception is Cancel while busy: a disabled button, not a followable link (S6).
- Use a pronoun in any shipped word. Use the person's name, "the chair", "the registrar".
- Print a date in any form but ISO, or "today" from anything but `QuotaCalendar.Today(TimeProvider)` /
  `ProgrammeCalendar.DateOf(utc)`. Dates are **typed** in the browser's date input (`InputDate`, F3).
- Print an exception's text on a page. An unexpected failure is a fixed sentence, and the failure goes to the log
  (T329, T338). A handler's own refusal (an `InvalidOperationException` or validation message written in Wombat's
  words) still shows (S4).
- Offer, draw or mention deferral (D53). "Outcome Deferred" is a progression category and stays.
- Change a seed, the credit engine, the agenda planner's rules, or the review page (`ReviewDetail.razor`, flow 07's),
  except where your lane says so.

`AuditPipelineBehavior` commits a failed handler's staged rows (T201): every command validates first, and stages only
once nothing can refuse it.

## How
- Work test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density.
  - This codebase comments *why*, with task ids. Cite T364 and the item that asked, e.g. "(T364, F1)",
    "(T364, round 3 M6)", "(T364, C12; T260)".
- Layers:
  - Application handlers go through `IApplicationDbContext`.
  - Web components use `IScopedSender` and DTOs, never Domain types in `.razor`.
  - A read about a trainee or a panel goes through the existing scope helpers (`CommitteeTraineeScope`,
    `PanelAdministrationReach`, `CommitteeReviewReadAccess`). An out-of-scope or unknown id reads as not found (null).
- Strings:
  - Use straight apostrophes in app strings.
  - Every sentence that counts has its singular ("1 seat", "1 member").
- Tests:
  - Change a test deliberately; never loosen it to pass.
  - `DefinedClassTests` fails any class a page uses that `app.css` does not define. Use the shared classes as given, and
    define your own under your heading.
- Each lane adds its classes under its own `/* Flow 08: … (T364) */` heading in `app.css`. Scope every phone rule to
  `.committee-calendar`.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`; the runbook lane
  does both after the merge.
  - T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. A new route needs a runbook step, so if the guard
    fails only for `/committee/reviews/new`, say so: the runbook lane adds it.
  - Name in your report every runbook line whose Expect your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release`: clean, with no new warnings.
  - Then run all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web,
    Integration (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message goes to the integrator, not to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and what another lane or the integrator must know:
  - the contracts you provide or rely on: exact type, member and parameter names, and how they differ from
    `build-contracts.md`;
  - files you touched outside your list;
  - runbook steps whose Expect your change alters.

## The lane plan

Three lanes in one wave, then the runbook lane.
- **Worktrees:** under `C:\dev\Wombat\.claude\worktrees\<branch>`.
- **Branches:** the integration branch is **`t364`**, which already holds the shared pieces (`94e14fee`,
  `build-contracts.md` § 0). Each lane is cut from `t364` and merged back into it. The whole is squashed into one commit
  on master. Nothing is pushed.

**Why this shape.** The pages share little but the pieces already on `t364`:
- the schedule page's address (`ReviewLinks.Schedule`, `ScheduleReturn`);
- the pager's label;
- the calendar's shared classes.

Each lane owns one page group from the handler to the CSS, so they run side by side. No migration is needed.

### Lane P "the panels" (`t364-panels`)

Covers Decision panels, `/committee/panels/new` and `/committee/panels/{PanelId}`, at 1280, 768 and 390.

**Scope.** Boards `R2-1-*` and `R2-2-*`; Q5, Q6, Q8; C10–C13; round 3 M6, M7, M8 (the panels list and Who decides each
EPA), F2 (the Administrator's filter: Show), S3, S4 (the panel's refusals), S6, S7 ("1 seat"); T260, T329 (the routing
never says "no routing" while it loads), T324 (the panel's field messages), T338 and T272 on these pages.
- **The list.**
  - Name, Scope in words (never the enum), Speciality, Members by name, and Open reviews.
  - Institution, for an Administrator, who chooses an institution first (FilterBar, Show).
  - Why New panel is missing, when it is (T260).
  - Who decides each EPA, never empty while it loads.
  - Both tables are `.clinic-table--stack-wide`, with captions.
  - Loading and load error (StatePanel with `OnRetry`).
- **A panel's page.**
  - The panel is named in its h1 and tab; there is no "Back to panels".
  - "About this panel" (DetailCard): scope, speciality, institution, and the open reviews, linked.
  - Name, scope, institution and speciality are fixed at creation and shown read-only (`.readonly-value`, Q8).
- **The seat picker** (Q5) replaces both native multi-selects.
  - One `.choice-row` fieldset per committee member: "Not seated / Member / External member", with the name over the
    whole address (`.person-cell`).
  - Chair is a select of the seated.
  - A panel of one is refused as a group (`.choice-group.is-invalid`, `aria-describedby`, the summary link focusing the
    first radio).
  - `Design/MultiSelectOptionKeyTests` changes with the control: rewrite it for the picker, or retire it with a reason.
- **Can no longer sit** (M6, C12, T260).
  - A member who can no longer sit appears in no control, and the warning names that member.
  - After Save panel the warning is re-read from the saved panel; it is a live alert the first time it shows.
  - "Panel members updated." takes the focus.
  - A new panel lands on its page with "{Name} was created. …" (`?created=`).
- **Decides for** (C13, F5).
  - Read-only for a speciality admin.
  - Locked while a review is open, with the warning shown first, so 4.13 reads the warning.
  - The race refusal stays beside the select, which goes back to the saved value; draw it as a scratch state.
- **Not found** is flow 01's page (M7). Access denied for the Coordinator is unchanged.
- **Data** (the Spec's list and round 2's):
  - member names and the speciality on the list;
  - a panel's open reviews (id, period, state) with `MayOpen`;
  - the members who can no longer sit, by name as stored.

**Files owned.**
- Application: `src/Wombat.Application/Features/CommitteeDecisions/{ListDecisionPanels,GetDecisionPanelById,
  GetDecisionPanelFormOptions,ListPanelMemberCandidates,CreateDecisionPanel,UpdateDecisionPanel,SetDecisionPanelBody,
  GetCommitteeRouting,DecisionPanelMemberRules,PanelAdministrationReach}.cs`, and their records in
  `CommitteeDecisionDtos.cs`.
- Web: `src/Wombat.Web/Components/Pages/CommitteeDecisions/{PanelsList,PanelEdit}.razor` (split into components under
  `Components/Pages/CommitteeDecisions/Panels/` if it helps).
- Shared: `app.css` (`/* Flow 08: the panels (T364) */`); DESIGN.md (the seat picker in § Form system; a panel's page in
  § Page-level patterns).

**Tests.**
- Application: the list's new members and scope; open reviews; the can-no-longer-sit names; the refusals' words.
- Web: `PanelDecisionBodyTests`, `PanelEdit*Tests`, `PanelPagesTraineeFirstTests` and `PanelsList*` (rewrite where the
  page changed).
- NEW per state named on the boards: loading never says no routing; created; members updated with the focus; the
  warning re-read; the race.
- `Design/MultiSelectOptionKeyTests`.
- Integration: the list on Postgres if a query changed shape.

### Lane D "Decisions due" (`t364-due`)

Covers `/committee/decisions-due`, at 1280, 768 and 390.

**Scope.** Boards `R2-3-*`; Q2, Q4; C2, C3, C9, C10, C11; round 3 F1 (the row's Schedule carries the row's own period),
F2, F6 (Period's help), M1–M3 (from the data: the planner and the seed already give the cadence; check that the words
match), M8 (By EPA), S4 (the subtitle drops "each semester"; Status keeps today's order; rows sort by surname), S7
("Open check-in #6 of Sipho Ndlovu"; "—" read as "None"); T259, T264, T280 (the pager's label "Decisions per page"),
T338 (an Administrator's failed institution list says so, with Try again).
- **The Trainee filter** (FilterBar, Show, Clear filters once a filter is set): "Every trainee", then the current
  registrars in the reader's scope, by surname.
- **The trainee line** (C2, `.trainee-line`), above the table while one trainee is filtered: one line for each panel
  that can hold the period. It reads one of three things:
  - the holding review ("Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on
    2026-10-12." with "Open review #1");
  - none ("No open review holds 2026 S2." with "Schedule a review", filled through `ReviewLinks.Schedule`);
  - an open formative check-in ("Formative check-in #6 is open: it takes no seat.").
  - The line reads the same seat rule the schedule's create runs (`CommitteeReviewSeats`); never a second copy.
- **Rows.**
  - Each row carries one action: Open review #n when held, Schedule when it can be scheduled (C3).
  - The holding sentence names the panel the holding review sits before (T259).
  - Each row's Schedule goes to `ReviewLinks.Schedule(panel, trainee, row period, returnTo: this page with its
    filters)`, a Missed row of an ended semester included (F1).
  - Status cells use `.status-cell`.
- **By EPA**: inside `.rung-legend`'s fold with `.fold-show` / `.fold-hide`, its summary 44 px; counts per state from
  the rows.
- **Stacking.** The list is `.clinic-table--stack` below 641 px. By EPA is `.clinic-table--stack-wide`.
- **The pager**: `PageSizeLabel="Decisions per page"`. After a page change or Show, the focus goes to the "Decisions"
  heading, which reads "Decisions, page 2 of 4" (`.list-section-page`), with the count under it (`.list-section-count`).
- **Loading and load error**: the header at once, a skeleton, then content, or "Could not load" with Try again.
- **Data**:
  - the holding review's id and its panel's name on each row (T259);
  - the trainee line's read (per trainee, period and panel: the open binding review, with its id, type and scheduled
    on, and any open formative check-ins);
  - the Trainee filter's options;
  - the surname to sort by.

**Files owned.**
- Application: `src/Wombat.Application/Features/CommitteeDecisions/GetEntrustmentDecisionsDue.cs`, NEW
  `GetTraineeSeatLine.cs` (or the name you give it, stated in your report), and their records in the DTO files.
- Web: `src/Wombat.Web/Components/Pages/CommitteeDecisions/{DecisionsDue.razor,DecisionsDueText.cs}`.
- Shared: `app.css` (`/* Flow 08: Decisions due (T364) */`); DESIGN.md (Decisions due's section; § Pager's focus rule).

**Tests.**
- Application: `EntrustmentDecisionsDueHandlerTests` (the holding panel; the surname order; the trainee filter); the new
  read (held, none, check-in; out of scope is empty).
- Integration: `EntrustmentDecisionsDuePostgresTests`.
- Web: `DecisionsDuePageTests` (rewrite), plus the trainee line's three states, F1's link, the focus after a page change,
  and the load error.

### Lane S "the schedule and Committee reviews" (`t364-schedule`)

Covers `/committee/reviews/new` (NEW) and `/committee/reviews`, at 1280, 1024, 768 and 390.

**Scope.** Boards `R2-4-*` and `R2-5-*`; Q1, Q3, Q9; C4–C8, C14; round 3 F1 (the form accepts an ended period and says
so under Period), F3, F4, F5, F6, M5, M9, M8 (Committee reviews), S1, S2, S4, S6, S7; T309 (item 1: the held seat
before Create; item 3: formative folds into Review type), T324, T338, T272.
- **Split `ReviewsSchedule.razor`** into:
  - the list at `/committee/reviews`, Committee reviews: Type with "Formative check-in", no Mode column, Period over its
    window (`.period-window`), stack-wide, captioned, Scheduled one badge colour on both lists; the header button
    "Schedule a review" for the five schedulers, linking `ReviewLinks.ScheduleNew`;
  - **the schedule page** at `/committee/reviews/new`, NEW, owned by Committee reviews in `NavOwners`, with its own
    title, trail and h1 "Schedule a review".
- **The form.**
  - Panel. Trainee: disabled until a panel is chosen, with its help; current registrars with their training year (5.29).
  - Period: the six of `CommitteeReviewPeriods.Around`, the same as Decisions due (M5).
  - Evidence window in the browser's date input (F3).
  - Scheduled on.
  - Review type as radios (`.scale-choices .form-check`), **preselected by `CommitteeReviewTypes.DefaultFor`**, with each
    type's help. C1's pre-graduation help reads: "The final review before the programme ends: it decides the trainee's
    progression, and whether they may graduate." "Formative check-in" is one of the types (no Formative only tick).
  - Filled from `?panel=&trainee=&period=`, and `return=` read through `ReviewLinks.ScheduleReturn` for Cancel.
  - An ended period says under Period: "2026 S1 has ended. A review can still sit for it: sittings are often held after
    the period ends." (F1).
  - In S2, Period's help adds F6's line.
- **The Agenda aside** (`.agenda-preview`). Beside the fields from 1100 px, under them below 1100 px (M9).
  - One narrow live region (`.agenda-live`) with no `role="alert"` inside it.
  - **Already held** before Create (Q3, T309 item 1): "Already held" with "Open review #n" and the panel and date. Create
    is not offered while the seat is held. The read runs the create's own seat check (`CommitteeReviewSeats`), never a
    second copy.
  - The pre-graduation "Already decided" lines.
  - NothingToDecide: the preview says so first, and Create refuses with `ScheduleCommitteeReview.NothingToDecide`.
  - "A formative review carries no agenda", and a check-in takes no seat.
  - The preview's failure reads "The agenda could not be read just now." with Try again, never the exception's text, and
    Create stays offered (C6).
- **Create** (C7).
  - Busy reads "Creating…" (`.is-running`), with Cancel a disabled button.
  - Each refusal goes in a summary above the actions and beside its field, with the focus on the summary, in Wombat's
    words. T324: "Choose a trainee.", "Choose a panel.", the evidence window's sentences.
  - The duplicate's race refusal names the trainee and says "scheduled" once ("Review #1 already puts Lerato Molefe
    before Paed Annual Review Panel for 2026 S2 (scheduled on 2026-10-12). …"), with Open review #1. Change
    `ScheduleCommitteeReview.AlreadyScheduled` and its tests.
  - Created: flow 07's review page, with the focus on its h1.
- **`CommitteeDecisionWording`**: `FormativeTypeShortLabel` (or what the list prints) reads "Formative check-in" (C8).
  Update the tests that pin it.
- **Data**:
  - the held seat for the page, as a query independent of the preview;
  - the default type and each type's help;
  - the trainee picker's training year;
  - pseudonyms as stored on the list.

**Files owned.**
- Application: `src/Wombat.Application/Features/CommitteeDecisions/{PreviewCommitteeAgenda,ScheduleCommitteeReview,
  ListSchedulableTrainees,ListReviewsForPanel,GetCommitteeReviewsAccess,CommitteeReviewPeriods,CommitteeDecisionWording,
  CloseFormativeReview,CommitteeReviewSeats}.cs`, and their records in the DTO files.
- Web: `src/Wombat.Web/Components/Pages/CommitteeDecisions/ReviewsSchedule.razor` (becomes the list), NEW
  `ScheduleReview.razor` (the page; split into components if it helps), `ReviewRowNames.cs` (the list's names only);
  `src/Wombat.Web/Navigation/{NavItems,NavOwners}.cs` (the new page's owner and title only).
- Shared: `app.css` (`/* Flow 08: the schedule and Committee reviews (T364) */`); DESIGN.md (§ Page-level patterns · Form
  page; the owner table's new row; § Table system's one-action rule; § Form system: Review type, dates).

**Tests.**
- Application: the preview's held seat (held, free, a check-in takes none, out of scope is nothing); the default type;
  the duplicate's words; NothingToDecide in the preview.
- Integration: `CommitteeReviewRacePostgresTests` if the create's words changed.
- Web: `ReviewsSchedule*Tests` (AgendaPreview, ListScope, PanelList, Prefill, TraineeFirst, TraineePicker), rewritten
  for the two pages. NEW for: F1's sentence; the preselected type; the preview's failure; the refusal summary and focus;
  busy; Cancel's return (a forged `return=` goes to Committee reviews).
- `CommitteeReviewStateLabelTests`; `Navigation/*`, `Hosting/PageTitleTests` for the new route; `Accessibility/*`.

### After the wave (the integrator, then the runbook lane)
- **Merge** P, D, then S into `t364`.
  - Settle `CommitteeDecisionDtos.cs` and `app.css` conflicts by keeping every lane's section.
  - Run the six suites.
  - Make DESIGN.md one account, its banner naming flow 08.
- **Runbook-and-docs lane** (`t364-runbook`), from the built code:
  - every Route, Do and Expect `R2-0-steps` lists for the 30 steps;
  - 4.1 and 4.2 as M10 restores them, 4.8 and 4.13 reading the warnings (F5);
  - the steps outside the flow (2.35, 2.37, 4.14, A.1.14, A.7.5–A.7.8, A.7.14; every "Formative only" and "Mode");
  - `coverage.md` (`/committee/reviews/new`), `states.md` § Committee rows;
  - `design/flows/08-committee-calendar.md` and any other flow brief that quotes a changed Expect.
- **The build's review**: three read-only reviewers on Sonnet:
  - code, data and authorization;
  - the pages, the design system and accessibility;
  - the runbook and the cast.
  - Then `build-review.md`, one fix pass (`t364-fix`), and the squash onto master as one T364 commit with the six
    suites' counts. Not pushed.
- **The squash names the task symptoms the build closes:** T309 items 1 and 3; T259; T260; T329's panels half; T338's
  and T272's pages here; T324's panel and schedule messages; T280's pager label and these tables' captions; T264's
  focus on Decisions due. The replay (step 7) confirms them before any task is closed.
