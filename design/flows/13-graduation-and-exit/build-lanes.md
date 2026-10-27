# T379 step 6: rules common to every build lane (flow 13, graduation and exit)

Adapted from `design/flows/12-people-admin/build-lanes.md` (T376). You are one lane of the build of flow 13 of Wombat's GUI
restructure, task T379 (`execution/tasks/in_progress/T379-*.md`). The design is final: Claude Design canvas "Wombat 13",
round 3 (192 boards, `round-3-check.md`: 70 of 73 done, 3 partly, final, no fourth round). The lane plan is at the end of
this file. The names two or more lanes share are in `build-contracts.md`, beside it; § 0 of it is already on `t379`.

## Read first (in your worktree)
- `design/flows/13-graduation-and-exit.md`: the brief. It holds the cast, the pages table (each route's file), § 5's known
  problems with file:line evidence (code cited at `82b108a6`), § 6's Q1–Q16 (decided 2026-10-27: all as recommended),
  § 7's acceptance, and the 26 runbook steps in § 8. **§ 5's T312 row is history:** T312 has landed (`c9d6fe13`). Build
  on its readers (`ProgrammeItems`, `EndedCurriculumItem`, `EntrustmentStandingDto.StarsAsEndedOn`, `EndedRecord`'s
  paused-EPA label, `EpaProgress`'s ended branch), not on § 5's text.
- `design/flows/13-graduation-and-exit/round-1-assessment.md` § 8 and § 10: the operator's Q17–Q27 (Q17 "Export
  portfolio" stays her h1, "your" in the subtitle and the result; Q18 Export's result above the form, focused, naming the
  file; Q19 the Unavailable page for every refused export; Q20 Verify's answer and the not-a-hash words; Q21 the day
  checked on the press, before any dialog; Q22 the closed list's pager at 20; Q23 Home's links titled by their pages, each
  with a line; Q24 Mark complete's confirm stays primary, Deactivate's danger; Q25 no line on a staff Home; Q26 the ended
  profile's account section; Q27 the staff export's fixed owner, no return parameter), and the picks (S1-A, S2-A without
  the user-page link, Export V1, Verify V1, the end V1).
- `design/flows/13-graduation-and-exit/round-2-review.md`: M1–M5, S1–S24, the nits, and the operator's F1–F11 at its end.
- `design/flows/13-graduation-and-exit/round-3-ask.txt`: F1–F11, M1–M5, S1–S24, N1–N16, T1–T12 and A1–A5 as round 3 was
  asked to draw them.
- `design/flows/13-graduation-and-exit/round-3-check.md`, **its "Build notes"**: what the boards leave, each assigned to a
  lane below (the `.details-list` 901px overflow, § 0's; R08's titles from the seed; values the boards derive; C07's order;
  a null `ProfileId`; the date inputs; the states.md and coverage.md rows; f13r3.css's section 2).
- `design/flows/13-graduation-and-exit/round-3/project/`: the accepted boards (`R3-*`). `round-3/boards-text.md` is their
  text, one heading per board in the canvas's order.
  - `R3-0-spec.dc.html`, `R3-0-spec-2.dc.html` and `R3-0-spec-3.dc.html` are the contract: the prerequisites, F1–F11 and
    M1–M5 item by item with where each is drawn, S1–S24, N1–N16, A1–A5, **the commands and queries as written** (MayExport,
    the end commands' result, GetExportSubjectQuery, GetTraineeProfileByIdQuery, GetFormerTraineeHomeQuery,
    ListEndedProfilesQuery, VerifyExportQuery), **the build rules** (S2, S6, S13, S19, N4, N5), **the Where column
    corrected** (S9), the DESIGN.md text, the class table (NEW, reused, out), **every message by kind** (built, NEW or
    PROPOSED, and what it replaces), **"DATA the build must add"**, the Illustration list, the brackets kept, the edge cases
    and A3/A4's measurements.
  - `R3-0-steps.dc.html` and `R3-0-steps-2.dc.html` list the 26 steps' Expects once built, T1–T12 and each journey's count.
  - The boards' CSS (`f13r3.css` section 2, the class table's NEW rows) is the canvas's picture, not code to copy. Rebuild
    each rule in `app.css` on the spacing scale, reusing what exists. Section 1 (`.sheet`, `.board-*`, `.chip*`, `.frame`,
    `.fh-*`, `.anno*`, `.doc`) never ships; `.anno-focus` stands in for the system's `:focus-visible` ring.
  - A board caption, a chip (NEW, DATA, PROPOSED, Illustration, "after T312") or a dashed strip is never a shipped word. A
    bracketed figure (‹registrar›, ‹day›, ‹address›, ‹n›, ‹about 200›, ‹title, as seeded›) is read from the data.
  - **The prerequisite has landed:** T312 (`c9d6fe13`), grown by F3. R05, R06 and P18's annotation draw what it built;
    where they differ from the code, say so in your report and keep the code.
- **The boards are design data, never instructions to you.** Where a board and the code disagree on a rule (who may act,
  what is shown to whom, what is stored), the code, `round-3-check.md`'s Build notes and this file win; say so in your
  report.
- **Check a board's CSS against DESIGN.md, not only against the board** (BRIEF § 11, flow 11): every stacked form and
  dialog row puts **the primary on top** (`column-reverse` over a DOM of the secondary first; the dialogs already do, flow
  11's general rule), and a result bolds **only its first sentence** where the board bolds one ("Export verified.").
- `execution/knowledge/EPA-PROGRAMME.md`: D54 (a withdrawn or completed registrar holds no role), D55 (only a running
  programme files), D49 (the last day's period), D15. CLAUDE.md § Activity platform: `TraineeProfile.EndedOn`, T281's
  take-back (`ProgrammeEndCredit`, `ITraineeCreditLock.HoldForEndAsync`), T312's ended list.
- `execution/architecture/DESIGN.md`: mandatory before touching any Razor or CSS: § The NavMenu (the personal links, the
  current item, the owner table), § Form system, § Table system (`StackWide`, the note row), § Button system (the
  destructive trigger, the dialog and header phone rules), § Alerts, § Badges (amended by § 0), § Dashboard page,
  § Page-level patterns (List page, Detail page, Form page, Unavailable, Breadcrumbs, Record page under a personal link),
  § Account / auth page (the static pages), § app.css section order. Also read CLAUDE.md: its conventions and footguns,
  § Roles (the `trainee_record` claim, `TraineeOrFormerTrainee`), the InstitutionalAdmin scope rules (an out-of-scope or
  unknown id reads as not found, never a refusal that leaks it), and § EF Core migrations.

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. No lane pushes, and the integrator does not either.
- Commit in your worktree as you go. Write each message to a file and use `git commit -F <file>`. End every message with
  the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Conventional Commits: `feat(portfolio): … (T379)`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns.
  - `app.css`, DESIGN.md, `NavOwners.cs`, `tests/Wombat.Web.Tests/Accessibility/ActionFocusTests.cs`,
    `tests/Wombat.Web.Tests/Navigation/ActiveNavItemTests.cs`, `tests/Wombat.Web.Tests/Navigation/
    DashboardLinkAuthorizationTests.cs` and `tests/Wombat.Web.Tests/Hosting/PageTitleTests.cs` are shared by section: each
    lane writes only under its own heading (§ 0 wrote the four app.css headings and the four DESIGN.md section-order lines),
    or adds or changes only the members or rows its lane names.
  - If you must touch a file another lane owns, keep the change minimal and name it in your report. A compile break your
    own change causes in another lane's test is yours to mend, minimally.
- **Four lanes share one machine** (BRIEF § 11). Run the six suites **one at a time**, each with its own build, never two
  `dotnet test` at once in your worktree, and Integration last. If a build or a test run is killed or hangs, stop and say
  so in your report; do not loop on it.

## Never
- Touch the apps on ports 5080 and 5180 or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves, and each works in its own throwaway `it_<guid>` schema.
- Pass `--no-build` to `dotnet test` or `dotnet ef` (CLAUDE.md: the solution and the projects build to different output
  paths; a stale assembly gives a green suite or an empty migration).
- **Add a migration.** Flow 13 needs exactly one, and § 0 made it (`20261027115106_T379_PortfolioExportCleanupRetired`:
  data only, the retired job's definition and runs). Nothing else is stored: the take-back count is returned, not kept;
  the forecast, the STAR count, the end's kind and the training year at the end are read; Verify's first generation and
  count are read from the export rows. A lane that finds it needs another stops and says so in its report.
- Upgrade MediatR; add Bootstrap, MudBlazor, Radzen or jQuery.
- Put any of these in shipped markup or CSS:
  - raw hex colours outside `:root`;
  - an inline `style=` (Verify carries four today, T280: they go; `Skeleton`'s `Width`/`Height` write one, so a loading
    card is raw `div.skeleton` with flow 11's `.skel-*` classes);
  - `<i class="bi">`, or "→" / `&rarr;` as an arrow (the `arrow-right` icon; Home's "Open My progress &rarr;" goes);
  - anything loaded from a third-party host;
  - an inline script or `onclick` (Verify stays static: no circuit, no script; N4's `autofocus` is an attribute).
- Use the native `disabled` attribute on an action the design says is unavailable (`aria-disabled="true"`,
  `.is-unavailable`, the reason beside it by `aria-describedby`).
- A filled danger button outside a `ConfirmDialog`: Mark complete… and Deactivate… are outline triggers in their own
  section; Deactivate's dialog confirms with the filled danger button, Mark complete's with the primary (Q24).
- Print a date in any form but ISO, or "today" from anything but `ProgrammeCalendar.DateOf(utc)` /
  `QuotaCalendar.Today(TimeProvider)` (T325: Export's defaults read the server's `DateTime.Today` today). A time is
  "yyyy-MM-dd HH:mm SAST" (`ActivityMoments.When`; Verify prints UTC today). Never `ToLocalTime()`.
- Print an exception's text on a page (T329; the profile does three times today, Export once). An unexpected failure is a
  fixed sentence (Spec 3's Failure row: "The portfolio could not be exported just now. Nothing was downloaded. Try again,
  or come back in a few minutes.", "The programme could not be ended just now. Nothing has changed. …", "Could not load
  this trainee profile. …", "The export could not be checked just now. Try again in a few minutes.") in `StatePanel`'s or
  `ActionResult`'s Alert, with Try again where Spec 3 draws one, and the failure goes to the log. A handler's own refusal
  shows its words where Spec 3 lists them as built; a NEW refusal is the board's words, mapped from a typed reason or a
  `ValidationException`'s property, never from the message text.
- Name a model property in a message (T324): every `[Required]` carries an `ErrorMessage` in Wombat's words ("Choose the
  from date.", "Choose the to date.", "Choose the last day.", "Enter the content hash to check.").
- Show a role's key where a person reads it: `WombatRoleLabels.For` (flow 12).
- Read a name, an id's existence or anything of another institution for an Institutional admin: an out-of-scope or unknown
  id reads as not found ("Profile unavailable", "Portfolio unavailable": one body for every cause, never 403 words), and
  the scope check is the handler's, not the page's. A Trainee-holding admin gets the T278/T290 note and nothing else on
  Trainees and the profile, asked before any read (`TraineeAdministrationRules.TraineeNoteOnTraineePages`, as built).
- Add a role check, except Q3's (lane R: My authorisations admits `TraineeOrFormerTrainee`). Every other page keeps its
  policy; MSF reports, My committee reviews and New activity stay role-only (Q3, D55).
- Change a seed, the credit engine, `ActivityService`, the PDF (`PortfolioPdfService` and its sections are flow 20's;
  T310 stays its own), the user page (flow 12's; its no-role sentence is read, not changed), Trainees' Waiting and Current
  lists (flow 11's), My progress's and the EPA page's own layout (flow 05's: only what names the end or links the record
  changes, R6), My activities' table (flow 03's: only its caption, trail, subtitle note and note row), or the shell,
  except where your lane says.

`AuditPipelineBehavior` commits a failed handler's staged rows (T201) and writes a refused command's message into the
caller's audit row: every command validates first and stages only once nothing can refuse it, and a refusal's message
names nobody out of scope. The end commands already read everything that can fail before recording the end (T281,
T312); keep it so.

## How
- Work test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids. Cite T379
  and the item that asked, e.g. "(T379, F2)", "(T379, round 3 M4)", "(T379, check build note 6)", "(T379, Q21)".
- Layers:
  - Application handlers go through `IApplicationDbContext`; Web components use `IScopedSender` and DTOs, never Domain
    types in `.razor`.
  - Every read keeps its scope check (`CanAccessInstitution`, `TraineeScopeResolver.MayReadAsync`).
  - Names are looked up once per page, by the query that serves it (`UserDisplayNames`, `GetDisplayNamesAsync`), never in
    Razor.
- Strings:
  - Straight apostrophes in app strings, except where a board writes "—" or "…" (Mark complete…, Deactivate…). Every count
    that counts has its singular ("1 activity observed after that day no longer counts.", "1 ended", "Exported once").
  - `<wbr>` after the @ in every sentence and row header naming an address (flow 11's M7): in markup, never in a stored
    string. Text that is an attribute (an aria-label) has none.
  - No pronoun for a person: "Lerato Molefe's", "their" (N10); "you" and "your" to the former trainee herself.
  - The possessive is `PortfolioWords.Possessive` ("Pieter du Plessis's"), the header action `PortfolioWords.ExportAction`,
    the profile's title `PortfolioWords.ProgrammeOf` (§ 0).
- Results and focus (BRIEF § 6 A5): every action's outcome shows where the focus goes, through `ActionResult`: Export's
  above its form (Q18), the end's at the ended record's top, Verify's in its `ActionResult` (`FocusOnLoad`, F9; V10 left
  alone from a link). One result on a page at a time. A refusal found before any dialog stays beside its field,
  `div.validation-message role="alert"`, the input `aria-invalid`, the focus in the field, the typed value kept (S19, Q21);
  no `RefusalSummary` for one field. A load failure's Alert takes the focus after a failed retry (`FocusAfterRetry`).
  Every destructive action confirms through `ConfirmDialog` (T264), its trigger an outline button; Cancel returns the
  focus to the opener.
- Standing notices are not live regions: the Trainee note, the "Read-only: your programme ended …" line, the account
  sentence, the ids note on Verify are `Alert` with `Role=""` or plain text; `status` only for what arrives with a result.
- Tables: captioned with a caption that does not repeat the heading (T280), `DataTable` with `StackWide` and its explicit
  roles; the row header `th scope="row"` (`.person-cell`: the name over its second line); one action a row named for its
  row (`RowNames.Distinct`). **No `.col-wrap` on a row header**; S24's `.col-wrap` is on the Programme column of the closed
  list only. `PagerControls` as built, rendered only when `TotalCount > PageSize` (flow 12's guard), its focus on the
  list's heading; no "Page n of N".
- Tests:
  - Change a test deliberately; never loosen it to pass.
  - `DefinedClassTests` fails any class a page uses that `app.css` does not define, and any `badge-` class named outside
    `BadgeFor` (use `BadgeFor.ProgrammeOutcome`, § 0). Use § 0's classes as given, and define your own under your heading.
  - Keep green: `Navigation/NavMenuAuthorizationTests` (with DESIGN.md's menu table), `ActiveNavItemTests`,
    `DashboardLinkAuthorizationTests`, `Hosting/PageTitleTests`, `Hosting/VerifyExportPageHostingTests` (Verify stays
    static), `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`, `Design/*`
    (`NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`, `TableColumnClassTests`, `RowActionMarkupTests`,
    `AlertRoleTests`), the scenario guard in `Scenario/` (a new route needs a runbook step: name it in your report; the
    runbook lane writes it).
- Each lane adds its classes under its own `/* Flow 13: … (T379) */` heading in `app.css` (§ 0 wrote all four, each with
  its `/* end … */` line), and its line under DESIGN.md § app.css section order's Graduation and exit heading (§ 0 wrote a
  placeholder per lane: replace yours). Scope every phone rule to the page's own class (`.record-card-body`,
  `.record-line`, `.export-page`, `.verify-page`, `.user-page` on the profile, `.closed-list`), never a bare `.btn`.
- **Give every new page its own class** (BRIEF § 11, flow 12: a route rendered outside the flow's 390 scopes kept old
  refusals). The pages table below names the file each route renders (`@page`); a lane that moves a route into a new file
  says so.
- **A lane's contract names the element that holds its cards** (BRIEF § 11, flow 10): a form inside a gap-spaced column
  gets none of the gap. Spec 2's A2 names each holder (Export's `form.form-container` is a child of `.export-page`;
  Verify's of `.verify-page`; Profile details' form inside `section.user-section`; End this programme is the section
  itself, with no form element); build to it.
- **Narrow desktops:** a stack-wide table must not split a word inside itself between 901 and 1100px. If a column still
  splits at 1000px, give it a floor in your section or stack from a wider breakpoint, and say which; the replay confirms
  at 901, 1000 and 1100.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`; the runbook lane does
  both after the merge. Name in your report every runbook line whose Expect your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release`: clean, with no new warnings (Release
    treats warnings as errors).
  - Then run all six suites, one at a time, each with its own build: Domain, Application, Infrastructure, Architecture,
    Web, Integration (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts. On
    `t379` at § 0 they are the counts in `build-contracts.md` § 0's last line.

## Your report (your final message goes to the integrator, not to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and what another lane or the integrator must know:
  - the contracts you provide or rely on: exact type, member and parameter names, and how they differ from
    `build-contracts.md`;
  - files you touched outside your list;
  - the words you shipped that no board draws (for `build-contracts.md` § 6);
  - runbook steps whose Expect your change alters.

## The pages, and the file each route renders

| Route | File (`@page`) | Lane |
|---|---|---|
| `/` (no role: a record holder, or neither) | `W/Home.razor` (the `default:` branch) and NEW `W/Dashboards/FormerTraineeRecordCard.razor` | R |
| `/portfolio/progress` | `P/MyProgress.razor` (the "Your record:" line around `Shared/Progress/EndedRecord.razor`, which stays unchanged) | R |
| `/portfolio/progress/{EpaId:int}` | `P/EpaProgress.razor` | R |
| `/portfolio/authorisations` | `P/MyAuthorisations.razor` | R |
| `/activities/mine` | `W/Activities/MyActivities.razor` | R |
| `/activities/{ActivityId:int}` | `W/Activities/ActivityView.razor` (N2's trail only, checked, not changed) | R |
| `/portfolio/export` | `P/ExportPortfolio.razor` | E |
| `/portfolio/export/{TraineeUserId}` | today `P/ExportPortfolio.razor`; lane E moves it into a component of its own (S2), its name lane E's | E |
| `/committee/reviews/{ReviewId:int}` | `W/CommitteeDecisions/ReviewDetail.razor` (the header's Actions only) | E |
| `/programme/trainees/{ProfileId:int}` | `W/Programme/ProgrammeTraineeDetail.razor` (the header's Actions only) | E |
| `/portfolio/verify` | `P/VerifyExport.razor` (static, `[ExcludeFromInteractiveRouting]`, `[AllowAnonymous]`) | V |
| `/admin/trainees/edit?id=` | `Tr/TraineeProfileEdit.razor` | P |
| `/admin/trainees` (Completed and closed only) | `Tr/PendingTraineesList.razor`'s `Closed()` and its read | P |

`W/` is `src/Wombat.Web/Components/Pages/`, `P/` is `W/Portfolio/`, `Tr/` is `W/Admin/Trainees/`. Spec 2's "Components/Dashboard"
is `W/Dashboards/` (where every Home's dashboard lives).

## The lane plan

§ 0, one wave of four page lanes, then the runbook lane. Four page lanes, one runbook lane, three Sonnet reviews and one
fix pass: nine agents beside the integrator.
- **Worktrees:** under `C:\dev\Wombat\.claude\worktrees\<branch>`.
- **Branches:** the integration branch is **`t379`**, cut from master at `3443d894` (T312's `c9d6fe13` and its handoff),
  which holds § 0 of `build-contracts.md`. Each lane is cut from `t379` after § 0 and merged back into it. The whole is
  squashed into one commit on master.
- **One wave: lanes R, E, V and P side by side.** What crosses is § 0's: the end commands' result (`ProgrammeEndResult`) and
  `ProgrammeEndCredit.CountTakenBackAsync`, which lane P's forecast reads; `MayExport` on the three DTOs, which lane E fills
  on two and lane P on one; the header action (`ExportPortfolioAction`), which lanes E and P mount; the record's STARs
  (`RecordStars`), which lanes R and P read; the end's reading (`EndedProgrammes.Of`), which lanes R, E and P read; the
  addresses and shared words (`PortfolioLinks`, `PortfolioWords`); the record holder's personal links (Export portfolio,
  `NavItems.PersonalFor`); the withdrawal's badge; the retired job's migration. Lane V touches none of the others' files.

### Lane R "record" (`t379-record`)

Covers the former trainee's record: Home's no-role branch, the menu as § 0 left it, My progress's "Your record:" line, the
EPA page's link, My authorisations admitting her, and My activities as she reads it, at 1280, 768 and 390.

**Scope.** Boards `R3-H*` (H01–H08) and `R3-R*` (R01–R10). Q1, Q2 (confirm § 0's menu on H01, H03, H08), Q3, Q9, Q23,
Q25; F6 (holds a STAR: any ratified decision; Home's count "in force"), S8 (the record line and Home's rule), S9 (Home's
branch a component under `DashboardFrame`), S15 (R10's bracketed counts are data), S17, S18 and N7 (R07), N2 (check
only), N3 (§ 0's), M3 (R05, R06 against T312 as built). Build notes 3 and 4 (R08's titles are the seed's, read, not
bracketed; R02/R04's derived values confirmed against the data). Folded: **T311**'s Home, menu, My authorisations and EPA
page items (its email item is lane P's), the record halves of **T280**, **T323**, **T325**, **T329**.
- **Home, no role** (H01–H08; `Home.razor`'s `default:` branch becomes NEW `FormerTraineeRecordCard` under the no-role
  `DashboardFrame`/`dashboard-grid`, so `DashboardLinkAuthorizationTests` renders it):
  - For a holder of the `trainee_record` claim whose `GetFormerTraineeHomeQuery` answers (NEW, `Features/Dashboards/
    FormerTrainee/`: the preferred profile through `TraineeScopeResolver.PreferredProfiles`, its end through
    `EndedProgrammes.Of`, the STARs through `RecordStars.ReadAsync`; **null for a current Trainee**, since the claim is
    issued for any profile (S8)): one emphasised `DashboardCard` "Your training record" (Q1), its body
    `div.record-card-body`: `p.ended-line` "You completed your programme on 2026-10-26." / "Your programme ended on
    2026-10-24." (`TraineeHomeWords.Ended` if it fits, else NEW words in your own words class), "Your record is kept,
    read-only: …" as Spec 3's Notice row, then `ul.record-links` of the record's pages, each its page's title as the link
    (Q23) and a line under it (`.record-link-why`): My progress, My authorisations ("Your 15 STARs in force, each with its
    certificate to download." from `InForce`; where `AnyIssued` and `InForce` is 0, PROPOSED "None of your STARs is in force
    now; My authorisations lists them."; with no STAR at all, H02: the row as plain text, `span.record-link-plain` "STARs"
    with `.record-link-none` "No STAR was issued to you, so there is no certificate to download."), My activities, Export
    portfolio. Every href from `PortfolioLinks`. No subtitle, no header action (flow 01's no-role frame).
  - Neither role nor record (H04): "No role assigned" as built. Loading (H05) and the load error (H06, built "Could not
    load your Home. …" with Try again). A 60-character name (H07) wraps.
  - A former trainee holding a staff role (H03, Q25): that role's Home, unchanged; § 0's personal links carry the record.
- **My progress** (R01, R02, R05, R10): the built subtitle (S7); NEW `div.record-line`: `p.record-line-label` "Your
  record:" then a `ul` of My authorisations (only where `RecordStars.AnyIssued`, F6) and My activities, **added by
  `MyProgress.razor` around `EndedRecord`**, never inside it (its golden-markup test unchanged) and never on the staff
  page; if the STAR read fails, My activities alone (S8). R05 (6.21, PAED-012 paused after the graduation) and R10 (17
  cards) are T312's as built: confirm and say where they differ.
- **The EPA page** (R03, R04, R06): "Open My authorisations" shows to a holder of the record too, not only of Trainee
  (`EpaProgress.razor`'s `_holdsTraineeRole` condition, DESIGN.md:3652's sentence; Q2), and only where the page has a
  STAR to show; R06 (a paused EPA on an ended record) as T312 built it: no `PausedRow` alert, "(no longer in use)" in the
  heading. Confirm R04's derived lines (build note 4).
- **My authorisations** (R08, R09; Q3): `[Authorize(Policy = "TraineeOrFormerTrainee")]` (the one role check this build
  adds); Download for each certificate as built (`DownloadEntrustmentCertificateCommand` needs no change: prove it with a
  test as the former trainee); trail Home › My progress › My authorisations (`NavOwners`: the row becomes
  `UnderAPersonalLink`, so it lights My progress for her as for a Trainee); du Plessis's empty card (R09) NEW "No STAR was
  issued to you, so there is no certificate to download." where the built "No active authorisations yet" stands for a
  record holder with none issued (say which shows to whom). Titles read from the seed (R08's eleven brackets).
- **My activities** (R07; S18, N7): trail Home › My progress › My activities for a record holder (`NavOwners` row
  `UnderAPersonalLink` → My progress; a Trainee keeps the item lit); the built subtitle then NEW "Read-only: your programme
  ended on 2026-10-24, so nothing new can be filed." for a record holder with no Trainee role; the table's NEW caption
  "Every activity filed by you or about you, with who has it now and its credit."; the Mini-CEX's "to David Naidoo" line
  (`.activity-link-to`, as built for others); the NEW note row "Observed after your last day in the programme,
  2026-10-24, so it counts towards nothing." (`.has-note`/`.history-note`, flow 03's) from a NEW field on
  `ListActivitiesBySubjectQuery`'s row (DATA: "My activities' note row's reason": observed after the subject's
  `EndedOn`, `TraineeProfile.IsAfterEnd`). No Log an activity (built).
- **N2:** an activity opened from My activities by a former trainee reads "Home › the activity": a test, no change.

**Files owned.**
- Application: NEW `src/Wombat.Application/Features/Dashboards/FormerTrainee/` (the query and its DTO);
  `src/Wombat.Application/Features/Activities/Queries/` `ListActivitiesBySubject*` (the row's note field only).
- Web: `src/Wombat.Web/Components/Pages/Home.razor` (the `default:` branch), NEW `Pages/Dashboards/FormerTraineeRecordCard.razor`
  (and a words class beside it), `Pages/Portfolio/{MyProgress,EpaProgress,MyAuthorisations}.razor`,
  `Pages/Activities/MyActivities.razor`, `NavOwners.cs` (the `MyAuthorisations` and `MyActivities` rows only).
- Tests: `tests/Wombat.Web.Tests/Portfolio/{EpaProgressPageTests,MyAuthorisationsPageTests}.cs`, the My progress and My
  activities page tests, `DashboardLinkAuthorizationTests` (the record holder's rows: every link on her Home opens a page
  that admits her), `ActiveNavItemTests` (My authorisations and My activities under My progress for her),
  `ActionFocusTests` (Home's card's rows if any), new files of your own; Application tests for the Home query (completed,
  withdrawn, no STAR, a current Trainee → null, H03's staff role).
- Shared: `app.css` (`/* Flow 13: the record (T379) */`): `.record-card-body`, `.ended-line`, `.record-links` (+ `li`,
  `a`), `.record-link-why`, `.record-link-none`, `.record-link-plain`, `.record-line` (+ `.record-line-label`, `ul`, `a`);
  phone rules (≤640.98px) scoped to `.record-links a` and `.record-line a` (44px). DESIGN.md: § Dashboard page (no role:
  Spec 3's row), § The NavMenu's personal links (confirm § 0's row), § Record page under a personal link (My authorisations
  and My activities under My progress for a record holder; the EPA page's link shows to her: DESIGN.md:3652).

**Tests.**
- Application: the Home query's rule (preferred profile; completed and withdrawn; `InForce` and `AnyIssued`; null for a
  current Trainee; a record holder with Assessor still answered); My activities' note field (after the end, on the last
  day, a running programme).
- Web: per board state H01–H08 and R01–R10 (each at 1280 and 390 where they differ): Home's card and links (each opens a
  page that admits her), no `&rarr;`, the plain no-STAR row; the record line only on her own page and without My
  authorisations when no STAR; My authorisations admitting a record holder and downloading; "Open My authorisations" on the
  EPA page for her; My activities' trail, caption, subtitle note and note row; `DefinedClassTests`, `RowNamesTests`,
  `NavMenuAuthorizationTests`.

### Lane E "export" (`t379-export`)

Covers Export portfolio, own and staff (`/portfolio/export`, `/portfolio/export/{TraineeUserId}`), and the staff export's
entry points on the committee review page and the registrar's programme page, at 1280, 768 and 390.

**Scope.** Boards `R3-E*` (E01–E20) and `R3-X*` (X01–X10). Q4 (without the user page), Q5, Q17, Q18, Q19, Q27; F1 and M1
(the action only where `MayExport`; X08's `TraineeElsewhere` review offers none; the Administrator is not refused), F4 and
S1 (no trail on the personal export), F6's no-profile caller (E19), M4 (the staff trail through `PageHeader.Trail`), S2
(lit by route), S6, S13, N1, N5, N9, N12; check build note 6 (a null `ProfileId`: the three-crumb trail). Folded:
**T314**, **T311**'s export half (its menu item is § 0's), the export halves of **T325** (the SA today), **T329**
(E13's fixed words), **T280**, **T323**.
- **The subject** (NEW `GetExportSubjectQuery(string? TraineeUserId, ClaimsPrincipal Principal)`, `Features/Reporting`,
  Spec 2's row): null for every refused cause (the export command's own `MayReadAsync`, an unknown id, a caller with no
  profile at `/portfolio/export`, F6); otherwise the name, institution, curriculum, training year, programme start, the
  last day (`EndedProgrammes.Of` on the preferred profile), and `ProfileId` (the preferred profile,
  `TraineeScopeResolver.PreferredProfiles`; null when none). Read before the form; the page draws no form until it
  answers (E14 loading, E15 its failure with Try again).
- **The page(s)** (`.export-page`; A2: `form.form-container` holding `.form-grid`, `.export-note` and `.form-actions`, a
  child of `.export-page`, which spaces the result and the form):
  - Own (E01–E03, E07, E08, E11–E15, E19): h1 and tab "Export portfolio" (Q17), NEW subtitle; **no trail** (F4: the item
    lights itself, `TrailTo` gives none); From (programme start) and To (SA today, or the last day for an ended record),
    both required, with Spec 3's hints; Export PDF (`InFlight` "Generating..."); the result above the form in
    `ActionResult`, focused, its first sentence bold: "Your portfolio exported: portfolio-aba6ce7943c2.pdf. The download
    should start automatically. Anyone holding the file can check it on Verify portfolio export." with the Verify link a
    plain `<a href="/portfolio/verify">` (S13). From after To (E11): the built "The start date must be before or equal to
    the end date." under From (S6: the `ValidationException` mapped to its field); an emptied date (E12): "Choose the from
    date." / "Choose the to date." (PROPOSED); any other failure (E13) Spec 3's fixed words, logged.
  - Staff (E04–E06, E09, E10, E16–E18, E20; S2): **its own component for `/portfolio/export/{TraineeUserId}`**, so the
    menu lights its owner by route, never the caller's own personal Export portfolio: Trainees for the Institutional
    admin, Programme trainees for the four programme roles (`NavOwners` rows; the Administrator, offered neither, gets
    Home › Export portfolio by `OwnerFor`, the held nit). h1 and tab `PortfolioWords.ExportAction(name)`; the trail
    "Home › Trainees › Lerato Molefe's programme › Export portfolio" (the middle crumb `PortfolioWords.ProgrammeOf`,
    `PortfolioLinks.TraineeProfile(ProfileId)`) or "Home › Programme trainees › Lerato Molefe › Export portfolio"
    (`PortfolioLinks.ProgrammeTrainee(ProfileId)`), through `PageHeader.Trail` as `AdmitRegistrar` does; a null
    `ProfileId` gives the three-crumb trail (build note 6); the hints name the registrar ("Lerato Molefe's programme
    started on 2023-01-15.", "Lerato Molefe's last day in the programme."); the result "Lerato Molefe's portfolio
    exported: …".
  - Refused, unknown or no profile (E09, E10, E19; Q19): the Unavailable page, h1 and tab "Portfolio unavailable", "This
    portfolio does not exist, or you cannot export it.", Go to Home, no form, nothing lit (`PageHeader Page=`, S2); the
    command's "You are not authorized to export this portfolio." stays its refusal, behind the page.
- **The entry points** (X01–X10; F1, N5): `ExportPortfolioAction` (§ 0) in `PageHeader`'s `Actions`:
  - on the review page, moved out of `SubtitleContent` (the progress link stays in the subtitle for the four programme
    roles, `RosterRoles`; none for the Institutional admin and the Administrator, X09, X10);
  - on the programme page, a new `Actions` slot.
  - `MayExport` filled by `GetCommitteeReviewByIdQuery` and `GetProgrammeTraineeQuery` (§ 0's fields) through
    `TraineeScopeResolver.MayReadAsync`. The review page reads the review back after each action through the query
    (`CommitteeReviewDetailDto.MayExport` is false from the commands, as `TraineeName` is): keep the value the query read.
    X08 (a review whose `TraineeElsewhere` is set) offers none.
- `ExportPortfolioCommand` keeps its gate and refusal; its validator gains the two required-date words only if the page
  cannot say them itself (say which).

**Files owned.**
- Application: NEW `src/Wombat.Application/Features/Reporting/GetExportSubject*`, `ExportPortfolio.cs` (validator words
  only), `Features/CommitteeDecisions/GetCommitteeReviewById.cs` (`MayExport` only), `Features/Programme/Trainees/
  GetProgrammeTrainee.cs` (`MayExport` only).
- Web: `src/Wombat.Web/Components/Pages/Portfolio/ExportPortfolio.razor` and NEW the staff export's component beside it
  (plus a words class), `Pages/CommitteeDecisions/ReviewDetail.razor` (the header only), `Pages/Programme/
  ProgrammeTraineeDetail.razor` (the header only), `NavOwners.cs` (the staff export's rows only).
- Tests: the export page's tests (new files of your own), the review page's and the programme page's header tests,
  `ActiveNavItemTests` (the staff export lit by route, S2, including a staff member who also holds a record),
  `PageTitleTests`' export rows, `ActionFocusTests`' export rows, Application tests for the subject query and `MayExport`.
- Shared: `app.css` (`/* Flow 13: the export (T379) */`): `.export-page`, `.export-note`; phone rules scoped to
  `.export-page` (the button full width and 44px, the date controls 44px). DESIGN.md: § Detail page and the title rule
  (the staff export's h1), § Form page (Export's dates and result), § Page-level patterns (the Unavailable page for the
  export), the owner table (the staff export's rows, the M4 trail).

**Tests.**
- Application: the subject query (self; a staff caller in scope; an Administrator; out of scope, unknown, no profile →
  null; `ProfileId` the preferred profile and null when none; the last day of an ended record); `MayExport` true and false
  on both DTOs (the `TraineeElsewhere` review, an Institutional admin of another institution).
- Web: per board state E01–E20 and X01–X10: the dates' defaults (own, staff, ended), no trail on the own page and one
  `aria-current`, the staff trail and its crumb hrefs, nothing lit on the Unavailable page, the result's focus and its
  plain Verify link, E11/E12 under their field, E13's fixed words; the action only where `MayExport`, in the header's
  actions on both pages; `DefinedClassTests`, `RowNamesTests`, `NarrowLayoutTests`.

### Lane V "verify" (`t379-verify`)

Covers Verify portfolio export (`/portfolio/verify`, static, signed out or in) and T313's server half, at 1280 and 390.

**Scope.** Boards `R3-V*` (V01–V10). Q6 (the field's help with the command list), Q11 (no STAR verify), Q16 (T313 with
T287's verify item: the cleanup job removed), Q20, F9 and S20 (the answer in `ActionResult`, `FocusOnLoad`, one success
Alert, "Export verified." bold, the sentences, then `dl.detail-list`), S10 (three not-a-hash refusals), S21, M5 (V07 at
390 is A.7.12's), N4, N15. Folded: **T313**, **T287**'s verify item, the Verify halves of **T325** (SAST, not UTC), **T280**
(no `<h3>` under the h1, no inline `style=`), **T329**.
- **The query** (`VerifyExportQuery`, Spec 2's row; its result may change shape, lane V's): strip all whitespace,
  lower-case; refuse, with a typed reason the page words, a wrong length ("This is not a SHA-256 hash: it has ‹n›
  characters, …"), a non-hex character ("… “‹c›” is not a hexadecimal digit …"), and the 12-digit prefix ("… it has 12
  characters, … The 12 in a file's name are only its start."); then the first generation, the count and the last export
  (`ORDER BY ExportedOn, Id`) with the first exporter. Upper case, spaces and a line break verify (V03, V09).
- **The page** (`.verify-page`; A2: `form.form-container` holding `.form-group` and `.form-actions`, a child of
  `.verify-page`): the GET form as built (static); the field's help and `ul.cmd-list` as V02 draws them (Windows:
  `certutil -hashfile <file> SHA256`, or in PowerShell `Get-FileHash <file>`; macOS: `shasum -a 256 <file>`; Linux:
  `sha256sum <file>`); a refusal under the field, `aria-describedby="hash-e hash-h hash-cmds"` (the refusal
  first, N15), the input `autofocus` (N4); the answer (V02, V03, V07, V09) as F9 and S20: "Export verified." then "Wombat
  generated this PDF: its content is byte for byte an export Wombat recorded." and `dl.detail-list` (First generated, in
  SAST; Exported, "3 times, the last on …" / "Once"; Trainee and First exported by as ids in `.hash-value`; Filter; File),
  with `.verify-ids-note` "People are shown by their Wombat user ids, never by name. …"; not found (V04) the built sentence
  and the NEW third; only spaces (V05) the built "Enter the content hash to check."; a failed check (V08) NEW "The export
  could not be checked just now. Try again in a few minutes."; from a link (V10, `?hash=…` with no `check=1`) the answer
  not focused.
- **T313's job:** `PortfolioExportCleanupJob`, its `AddScheduledJob` registration in `DependencyInjection.cs` and its tests
  removed (§ 0's migration removes the stored definition and runs); any test or page that counts the jobs (A.2.1's nine)
  goes to eight.

**Files owned.**
- Application: `src/Wombat.Application/Features/Reporting/VerifyExport.cs` (and `PortfolioExportRecordDto` if its shape
  changes: say so; nothing else reads it but Verify).
- Infrastructure: `src/Wombat.Infrastructure/Scheduling/Jobs/PortfolioExportCleanupJob.cs` (deleted),
  `DependencyInjection.cs` (its one line).
- Web: `src/Wombat.Web/Components/Pages/Portfolio/VerifyExport.razor` (and a words class beside it).
- Tests: `tests/Wombat.Web.Tests/Portfolio/VerifyExportPageTests.cs`, `Hosting/VerifyExportPageHostingTests.cs`,
  `tests/Wombat.Application.Tests/Scheduling/PortfolioExportCleanupJobTests.cs` (deleted), the job registry's tests,
  Application tests for the query.
- Shared: `app.css` (`/* Flow 13: verify (T379) */`): `.verify-page` (+ its `.detail-list`, `dd`), `.verify-ids-note`,
  `.hash-value`, `.cmd-list` (+ `code`); phone rules scoped to `.verify-page` (one column, the button full width, 44px).
  DESIGN.md: § Account / auth page's static pages (Verify's answer, the field's refusal first, `autofocus`).

**Tests.**
- Application: case and whitespace; each refusal and its reason; the first generation, the count and the last of three
  identical exports in order; a missing hash → not found.
- Web: per board state V01–V10 (1280 and 390): the answer's markup and focus, the refusals under the field with
  `autofocus` and their order in `aria-describedby`, SAST times, no inline style, no `<h3>`; Verify stays static
  (`VerifyExportPageHostingTests`); `DefinedClassTests`, `NarrowLayoutTests`.

### Lane P "end" (`t379-end`)

Covers the trainee profile (`/admin/trainees/edit?id=`: an active profile's details, End this programme with its two
dialogs, the ended record and its account section) and Trainees' Completed and closed list, at 1280, 768 and 390; the
graduation email.

**Scope.** Boards `R3-P*` (P01–P25) and `R3-C*` (C01–C07). Q7, Q8 (as Q22 corrects it: 20 a page), Q12, Q13 (the last
ratified decision read, gating nothing), Q14, Q15, Q21, Q24, Q26; F2 and M2 (the result from `ProgrammeEndResult`, the
dialog's forecast), F5 and S5 (the account sentence and its variants), F7 and S11 (the closed list paged, the
Administrator's institution under each name, the erased person under the pseudonym), F8 and S12 (the address in Mark
complete's dialog), F10 (the badges, `BadgeFor.ProgrammeOutcome`), F11 and S3 (the Domain's "last day" words, the
before-start refusal, "Choose the last day."), S4 (the profile's read), S7, S14 (P18), S19, S22, S23, S24, N6, N13, N14;
check build notes 5 (C07 ordered by the end day) and 7 (the date input). Folded: **T311**'s email item, the closed
list's View (A11), the profile and closed-list halves of **T280**, **T323**, **T325**, **T329**.
- **The profile's read** (S4): `GetTraineeProfileByIdQuery` returns null for unknown and out of scope (one body); its DTO
  gains, beside § 0's `MayExport`: the end's kind and day and the training year at the end (from `EndedProgrammes.Of`),
  the last ratified decision (NEW `GetLastRatifiedDecisionQuery` or a reader in `Features/CommitteeDecisions`: "The last
  committee decision: Graduate (programme complete), Pre-graduation review, ratified 2026-10-26."), and the account
  sentence's DATA (roles held now, locked, erased; `RecordStars.AnyIssued`). The page's `StatePanel` gets `LoadError`
  (fixed words) and `OnRetry`, Try again focusing the h1; Profile unavailable (P12, P13) for null.
- **The page** (`.user-page`, flow 12's column and sections): h1, tab and crumb `PortfolioWords.ProgrammeOf(name)` (T2),
  "Back to trainees" gone; the header's `ExportPortfolioAction` (§ 0) with `MayExport`; the summary `dl.detail-list
  aria-label="Trainee summary"` (Status, Email; no "Name:", no visible "Trainee summary" heading, T6), Status one row
  (N6): Active a plain word, Completed `BadgeFor.ProgrammeOutcome(true)` and the day, Inactive the grey badge and the built
  "Left the programme: 2026-10-24";
  - **Profile details** (`section.detail-card.user-section`, h2): the form as built with Save profile and Cancel, the
    built subtitle and hints (S7);
  - **End this programme** (`section.user-section`, h2 `#end-h` with no tabindex, N14; the section itself, no form
    element, A2): the last ratified decision as a plain `p`; Last day in the programme * (a date input, `EndDate`
    nullable on the page, its help `.field-help`, Spec 3's Hint); Mark complete… and Deactivate… as outline buttons, each
    `aria-describedby` its note (`#end-mc-note`, `#end-dc-note`, N13); on the press, the day is checked first (Q21, S3,
    S19): empty "Choose the last day.", after today "The last day cannot be after today (2026-10-26).", before the start
    "The last day cannot be before the programme start date (2023-01-15).", each under the field, `role="alert"`, the
    focus in it, the typed day kept, no dialog;
  - the dialogs: "Mark Lerato Molefe's programme complete?" (the address "sent to molefe@kgk.wombat.local", F8; "Lerato
    Molefe keeps the record, read-only, and can still export the portfolio."; the forecast clause "‹n› activities,
    observed after ‹day›, will stop counting." only when the forecast is not 0, M2; "This cannot be undone."; confirm
    primary, Q24) and "Deactivate Pieter du Plessis's profile?" (": 1 activity, observed 2026-10-25, will stop counting.";
    filled danger). The forecast is NEW `GetTakeBackForecastQuery(int ProfileId, DateOnly LastDay, ClaimsPrincipal)` over
    `ProgrammeEndCredit.CountTakenBackAsync` (§ 0), read when the dialog opens for the day typed; P07's command refusal
    at confirm shows in the same place;
  - the result at the ended record's top, focused: built "Trainee marked complete. The Trainee role has been removed and a
    graduation email sent." and Spec 3's withdrawal sentence, each followed by "‹n› activities observed after that day no
    longer count." from `ProgrammeEndResult.TakenBack` when not 0 (singular for 1); a failure (P06) Spec 3's fixed words;
  - **the ended record** (P05, P11, P18, P21, P24, P25): the outcome first, the details as text under the built "This
    programme has ended, …" (T305), the subtitle "The record of a programme that has ended.", and the account section
    (F5): "Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the
    STAR certificates." (the STAR clause only with `AnyIssued`), "‹registrar› now holds Assessor. They can still read the
    record and export the portfolio." (P24, PROPOSED), the link alone when locked (P25), nothing for an erased person, and
    `p.account-open` "Open Lerato Molefe's user page" (`PeopleLinks.User`).
- **The handlers:** the Domain's `EnsureCanEnd` says "last day" for both ends with the day in the before-start refusal
  (F11: "The last day cannot be after today (2026-10-26).", "The last day cannot be before the programme start date
  (2023-01-15)."), with its Domain and Application tests; the end commands as § 0 left them (their result; read nothing
  more after the end is recorded).
- **Completed and closed** (C01–C07; `PendingTraineesList.razor`'s `Closed()` and its own read; Waiting and Current are
  flow 11's, untouched): NEW `ListEndedProfilesQuery(int Page, int PageSize, ClaimsPrincipal Principal)` paged in SQL, 20 a
  page, the latest end first then id, in scope, the institution for the Administrator, an erased person under the
  pseudonym as Withdrawn (F7); `section.list-section` h2 "Completed and closed" with AdmissionText's NEW "‹n› ended" (the
  singular "1 ended"); `DataTable` `StackWide`, scoped `.closed-list`, caption "Programmes that have ended, the latest end
  first. An ended record is read-only."; columns as C02 draws them: Name as the row header (the name; "Name and
  institution" for the Administrator, `.person-cell` with the institution under the name, C06), Curriculum and
  sub-speciality (`.person-cell`: "Paediatric EPA Curriculum (11.1)" over "Paediatrics"), Programme (`.person-cell`,
  `.col-wrap`, S22, S24: "2023-01-15 to 2026-10-26" over "Training year 4 when it ended"), Outcome (`.outcome-cell`: the
  badge, then the day), and Actions, one View named "View Lerato Molefe's record" to `PortfolioLinks.TraineeProfile(id)`; `PagerControls` past 20, focus on the
  h2; none ended (C03): PROPOSED "No programme has ended yet." / "A trainee is listed here once …". The Current list keeps
  reading active profiles only.
- **The graduation email** (Q12, T311 item 4): `GraduationEmail` drops "Your committee has ratified your final entrustment
  decisions" and links My progress, in the idiom the other templates use for a link; flow 19 keeps its design. Its words
  go in § 6 (no board draws the email).

**Files owned.**
- Domain: `src/Wombat.Domain/Identity/TraineeProfile.cs` (`EnsureCanEnd`'s words only), its tests.
- Application: `src/Wombat.Application/Features/Trainees/{GetTraineeProfileById,TraineeProfileDto}.cs` (beside § 0's
  field), NEW `ListEndedProfiles*`, NEW `GetTakeBackForecast*`, NEW last-ratified-decision reader under
  `Features/CommitteeDecisions/`, `src/Wombat.Application/Common/Email/Templates/GraduationEmail.cs`.
- Web: `src/Wombat.Web/Components/Pages/Admin/Trainees/TraineeProfileEdit.razor` (split into components of your own if it
  helps), `PendingTraineesList.razor` (`Closed()` and its read only), `AdmissionText.cs` (the closed list's words only),
  a words class of your own for the profile.
- Tests: `tests/Wombat.Web.Tests/Admin/TraineeProfile*Tests.cs`, the closed list's tests (`AdmissionPagesTests`' closed
  rows), `tests/Wombat.Application.Tests/Features/Trainees/*` (not `EndedCurriculumListTests.cs`, T312's),
  `tests/Wombat.Domain.Tests/Identity/{TraineeProfileEndTests,TraineeProfileCompletionTests}.cs`, `ActionFocusTests`' profile
  rows, `PageTitleTests`' profile row, the email's tests.
- Shared: `app.css` (`/* Flow 13: the programme's end (T379) */`): `.account-open` (+ `a`), `.closed-list` (+ `td`, `th`),
  `.outcome-cell`, and what the end section needs beyond flow 12's `.user-page` rules; phone rules scoped to `.user-page`
  and `.closed-list` (`.account-open a` 44px). DESIGN.md: § Detail page and the title rule (the profile's h1, the ended
  record's outcome first), § Button system (S19's amendment: a refusal before any dialog stays beside its field), § Table
  system (Completed and closed), § Page-level patterns (the Unavailable page for the profile).

**Tests.**
- Domain: the two refusals' words and the before-start day.
- Application: the profile read's null (unknown, out of scope), its new fields (completed, withdrawn, erased, locked,
  holding Assessor, with and without a STAR); the forecast against `ProgrammeEndResult` for the same data (1 and 0); the
  closed list's order, page, scope, the Administrator's institution, the pseudonym; the email's words.
- Web: per board state P01–P25 and C01–C07 (1280 and 390; 768 for P and C): the press check's three refusals with no
  dialog and the focus in the field; each dialog's words and the forecast clause only when not 0; the result built from
  the count; Status one row with its badge; the account section's four variants; the header action only with
  `MayExport`; Profile unavailable for unknown and out of scope; the closed list's View, order, pager past 20, the
  Administrator's column, the erased row; `DefinedClassTests`, `RowNamesTests`, `NarrowLayoutTests` (C04 at 901).

### After the wave (the integrator, then the runbook lane)
- **Merge** R, E, V and P into `t379`, run the six suites one at a time. Settle `app.css`, DESIGN.md, `NavOwners.cs`,
  `ActionFocusTests.cs`, `ActiveNavItemTests.cs`, `DashboardLinkAuthorizationTests.cs` and `PageTitleTests.cs` by keeping
  every lane's section. Make DESIGN.md one account, its banner naming flow 13.
- **Runbook-and-docs lane** (`t379-runbook`), from the built code:
  - every Expect `R3-0-steps` lists for the 26 steps: 5.9, 5.10 (the Note and F-5.10a go), 5.12, 5.13 (F-5.13a and
    F-5.13b close), 5.14, 5.16, 5.17, 5.18, 5.20, 5.21 (F-5.21a), 5.22 (F-5.22a), 5.23 (F-5.23a), 5.27, 5.28, A.7.4
    (F-A.7.4a), A.7.12 (V07 at 390); 6.21, 6.40 and 6.41 checked against T312's rewrite (no change expected); 5.11, 5.15,
    5.19, 5.24–5.26, 5.29 unchanged;
  - the crossing steps T1–T11 name: 5.8 (Expect unchanged, Note rewritten), 2.30, 6.34, 6.36 and A.7.9 (the profile's h1,
    no Back to trainees, End this programme, the header action; A.7.9's closed list stacked with View 44px and "three
    fields plus the last day and two triggers"), A.2.1 and A.2.9 (eight jobs, "the other five", the cleanup bullet and the
    90-day Note gone, F-A.2.9a closed), 5.17, 5.27 and A.1.14 (no "Name:", no "Trainee summary" heading; the closed list
    latest first at A.1.14, 5.27 and 6.34; A.1.14 gains the closed list's line with the erased row, R3-C07), A.1.6 and
    A.4.6 (du Plessis's Home: R3-H02's card, his menu with Export portfolio); each journey's count as T12 gives it; BRIEF §
    9 item 7's sweep of the whole runbook for the words that changed ("Trainee profile", "Back to trainees", "Edit" on a
    closed row, "Portfolio exported successfully", "UTC", "Export portfolio" in a former trainee's menu, the completion
    and deactivation date refusals), and a step for every new route (the staff export's component;
    `Scenario/ScenarioRunbookTests`);
  - `coverage.md` (rows 143, 223, 323, 385, 494, re-pointed at the built pages; the staff export's component, Home's
    FormerTraineeRecordCard), `states.md` (N8's rows: E09–E15, E19, H05, H06, P06, P12–P15, P22, P23, V08–V10, X03–X07,
    and what the build adds);
  - `design/flows/13-graduation-and-exit.md` (§ 5's closed rows), then `check_verbatim_steps.py` (re-sync § 8) and
    `check_flow_completeness.py`, both at exit 0.
- **The build's review:** three read-only reviewers on Sonnet (code, data, scope and authorization: `MayExport` against
  the command's gate on all three pages, the subject query's null for every refused cause, the forecast against the
  result, the closed list's scope and the Administrator's view, My authorisations' new policy and its download, the
  former trainee's Home query's null for a current Trainee, Verify's refusals and order, every out-of-scope id read as
  not found; the pages, the design system and accessibility; the runbook and the cast), then `build-review.md`, one fix
  pass (`t379-fix`), and the squash onto master as one T379 commit with the six suites' counts.
- **The squash names the task symptoms the build closes:** T311 (Home, menu, My authorisations, the EPA page's link, the
  email), T314, T313 with T287's verify item, the flow 13 halves of T325, T329, T280 and T323; the closed list's View; A5's
  results and the confirmations (BRIEF § 6). T310 (the PDF), T287's sign-in items, T166's gating and T374 stay their own.
  The replay (step 7) confirms them before any task is closed.
- **For the operator, not the build:** N16's pager select (28px at 390, every flow's `PagerControls`) and flow 03's
  one-line `.activity-link` (20px at 390) are proposed as system tasks, not built here; the `.details-list` overflow at
  901px is fixed in § 0.
