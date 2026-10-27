# Flow 13 build review: the runbook and the cast

Reviewer: read-only, Sonnet. Worktree `t379` at `95d3f765`, diff against `master` (`git diff master...t379`). Scope: the runbook
lane's commits `6ca1b6cd` and `95d3f765`: `act-5-graduation.md` (5.8 to 5.28, with 5.1 to 5.7 and the outcome SQL read for
carry-over), `act-2-onboarding.md` 2.30, `act-6-catalogue.md` 6.21, 6.34, 6.36, 6.41, `appendix-cross-cutting.md` A.1.6, A.1.14,
A.2.1, A.2.9, A.4.6, A.7.4, A.7.9, A.7.12, `coverage.md`, `states.md`, and `design/flows/13-graduation-and-exit.md` § 5 and § 8,
against the built code (the words classes, `.razor` files, handlers, `NavItems`, `NavOwners`, `TraineeScopeResolver`) and the cast.
Paths below are under `execution/knowledge/scenario-paediatrics/` unless stated.

Summary: **0 must-fix, 4 should-fix, 7 nit.** Every quoted Expect string the lane added matches what the code prints; the cast,
dates, counts, journeys and routes hold; no Actual line was rewritten.

## What was run

- `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj -c Release --filter FullyQualifiedName~Scenario` (built, no
  `--no-build`): **49 passed, 0 failed.**
- `python design/tools/check_verbatim_steps.py`: **424 verbatim steps; 0 problems** (flow 13's § 8 is held to the runbook).
- A script over all seven act files against `master`: **no Actual block differs** from master's (the lane only touched Expect, Note
  and Gap lines). Eight Gap lines changed, every one a "closed by T379" line (5.10, 5.12, 5.13, 5.21, 5.22, 5.23, A.2.9, A.7.4),
  and `coverage.md`'s T379 paragraph lists exactly those eight gap ids.
- `@page` files: 79; distinct templates: 88; `### Step` headings: 332 less the README's format example = **331 steps**. All three
  match `coverage.md`'s "331 steps, 79 page files and 88 templates".

## What was checked and holds

- **Words.** Every quoted string in 5.9 to 5.23, 5.27, 5.28 and the new states rows was matched against `ExportWords`,
  `VerifyExportWords`, `FormerTraineeRecordWords`, `TraineeProfileWords`, `AdmissionText`, `PortfolioWords`, `GraduationEmail`,
  `TraineeProfile.EndDayRefusal`, `LastRatifiedDecision` and the razor files. Spot checks by hand: the dialog's two paragraphs
  with and without the forecast (`AfterTheDay`: "…: 1 activity, observed D−1, will stop counting."), the result's "1 activity
  observed after that day no longer counts." (`TakenBack`), "Left the programme: D−2", the account sentence with and without the
  STAR clause (`AnyStarIssued`; Act 4 gave du Plessis none: 4.30 stages nothing), the refusal "The start date must be before or
  equal to the end date." (`ExportPortfolio.cs:26`), "Loading your Home." and "Could not load your Home." (`DashboardFrame`).
- **Cast and counts.** Molefe completes at 5.17 (J−3y to D, "Training year 4 when it ended", Completed D); du Plessis withdraws at
  5.27 (J−1y to D−2, "Training year 2 when it ended", Withdrawn D−2, forecast of exactly the one completed D−1 Mini-CEX; the
  cancelled DOPS and the submitted Portfolio review credit nothing). "4 current", "1 ended", "3 current", "2 ended" and "3 ended"
  (A.1.14: Ndlovu's erased profile, Withdrawn on the erasure day, under `deleted_user_…`; ties on D are broken by profile id
  descending in `ListEndedProfilesQueryHandler`, so his row is first, then Molefe's, then du Plessis's, as written). After D54
  neither holds a role; "No role" over the keep-an-account line on Users matches A.1.14. The exports: one file three times
  (5.9, 5.10, 5.12), then a second file once (5.23); "3 times, the last on …" at 5.13 and "Once" at 5.23; the outcome SQL's
  "two rows, 3 then 1" matches.
- **Authorization against each step's Role.** 5.10: an InstitutionalAdmin reaches Trainees, Edit, the header action
  (`MayExport` through `TraineeScopeResolver.MayReadAsync`) and `ExportTraineePortfolio` (owner Trainees in `NavOwners`). 5.12: a
  Coordinator holds an oversight role at KGK, so the review's header offers the action (`GetCommitteeReviewById` sets `MayExport`),
  and the owner is Programme trainees (`ProgrammeScope.RosterRoles` includes Coordinator). The states rows for Dr Naidoo acting as
  Committee member (R3-X05), Dr Mokoena and Dr Sithole (R3-X06, X07) are reachable: `HoldsOversightRole` and `IsOverseenBy` admit a
  CommitteeMember at the institution, a speciality admin and a sub-speciality admin. The ended profile's header action and the
  review's after 5.17 stay offered (`MayReadAsync` asks no current-trainee rule; `GetProgrammeTrainee` reads an ended profile).
  5.22: `MyAuthorisations` is `TraineeOrFormerTrainee`; `NavOwners` puts it under My progress (`UnderAPersonalLink`), so
  "Home › My progress › My authorisations" holds; the certificate handler judges by `MayReadAsync`, which admits the person
  herself (the lane added a test).
- **Routes.** `/portfolio/export/{TraineeUserId}` is `ExportTraineePortfolio`'s `@page`; the Route lines of 5.10, 5.12, 5.17,
  5.22, 5.23 and A.1.14 name real templates in the order the Do walks them; coverage's rows for `/admin/trainees`,
  `/admin/trainees/edit`, `/admin/users`, `/admin/users/{UserId}`, `/committee/reviews`, `/committee/reviews/{ReviewId:int}`,
  `/portfolio/authorisations`, `/portfolio/export`, `/portfolio/export/{TraineeUserId}`, `/portfolio/verify` and `/` list the
  steps whose Route names them (5.10 left `/admin/users`; 5.12 joined the two committee rows; 5.23 joined `/`).
- **Order and carry-over.** 5.9 before 5.10 and 5.12 (the same file), 5.13 after all three and before 5.23; 5.16's refusal before
  5.17's completion; 5.17 before 5.27 ("2 ended"); Act 6 reads "2 ended" at 6.34 (before the appendix erases Ndlovu) and the
  appendix "3 ended" at A.1.14 (after). The removed job appears only in A.2.1 and A.2.9; A.2.9's "other five jobs" and the
  outcome SQL's "eight, all true" and "eight keys; the digest twice" agree with the eight job classes in
  `src/Wombat.Infrastructure/Scheduling/Jobs/` and the migration's two DELETEs. The old Actual lines that say nine and six stay
  as history, with Gaps that say they predate the change.
- **Journeys.** 5.9 (2 presses), 5.10 (4 + 0, 5 at 390), 5.12 (4 + 0, 5), 5.13 (1 + 2), 5.17 (4 + 0, 5), 5.23 (2 + 0; 2 from Home's
  card at 390, 3 by the menu), 5.27 (4 + 1, 5 + 1) agree with the board's T12 table and with the page's controls.
- **§ 5 and § 8 of the flow brief.** § 8's pasted steps are verbatim (checker). § 5's rows name the same closed gaps as the steps
  and `coverage.md`.

## Findings

### R1: S: Step 4.33 still says Molefe's review "offers Smit no action"

`act-4-annual-review.md:936-937` (Step 4.33, Mr Smit reads the ratified schedule): "Molefe's review shows its decision, agenda and
STAR numbers, and offers Smit no action." Since T379 a Coordinator who oversees the trainee sees "Export Lerato Molefe's
portfolio" in that page's header (`ReviewDetail.razor` `HeaderActions`, `MayExport` from `GetCommitteeReviewById`; the runbook's
own 5.12 and states row `review-detail--export-action` say so). A replay agent reads Step 4.33 against the built page and finds a
control the Expect says is absent. The lane rewrote 5.12 and the states rows but did not sweep the Acts that read a review page as
staff. Fix: "…and offers Smit no Ratify or Start, only the header's Export Lerato Molefe's portfolio (T379, Step 5.12)". Steps
4.26 (Mbatha: "no Ratify…, no Remove… and no Action column") and 4.28 to 4.31 (chair) say nothing false, but could note the
header action once for the committee member (R3-X05).

### R2: S: 5.28 and 6.41 give the "counts towards nothing" note row to the Mini-CEX alone; the code gives it to every row observed after the last day

`act-5-graduation.md:841-844` (5.28): "A note row under it reads 'Observed after your last day in the programme, D−2, so it counts
towards nothing.'", and `act-6-catalogue.md:955-957` (6.41), say it of the Mini-CEX only. `ListActivitiesBySubjectQuery` sets
`ObservedAfterEnd = TraineeProfile.IsAfterEnd(endedOn, row.ObservedOn)` for every row of the caller's own list, whatever its state,
and `MyActivities.razor` draws the note row under each. The runbook itself says du Plessis has more such rows: Act 5's outcome SQL
note lists "Act 3's cancelled DOPS (dated D…) and his submitted Portfolio and Logbook Review (D−1)", and 6.41's Actual lists the
cancelled DOPS, the open Portfolio review, a CBD and the Teaching Session Logs beside the Mini-CEX. So a replay of 5.28 or 6.41
finds at least two more note rows (under a cancelled request and an open one, where "counts towards nothing" is odd copy as well as
unexpected). Fix: say "a note row under every activity observed after his last day: the Mini-CEX, the cancelled DOPS (dated D) and
the submitted Portfolio review (D−1), each reading …"; and if the copy under a cancelled or still-open filing is not wanted, that is
a finding for flow 13's owner, not the runbook.

### R3: S: A.7.12 and `verify-export--narrow` expect "the hash in the field wrapping", which a text input cannot do

`appendix-cross-cutting.md:1594-1596` (A.7.12): "…First generated `D` hh:mm SAST, the Trainee and First exported by ids and the hash
in the field wrapping", and `states.md` `verify-export--narrow` ("the ids and the hash wrapping"). `VerifyExport.razor` draws the
hash field as `<input type="text">`; a single-line input scrolls or clips its value and never wraps it. Only the answer's ids
(`class="hash-value"`) wrap. Check as written, the replay can only record a gap. Fix: "the Trainee and First exported by ids
wrapping in the answer, the hash kept on one line in its field". The same step's Do says only "verify Dr Molefe's portfolio PDF
from Act 5 by its hash" while the Expect describes the newest export (Step 5.23, "Exported Once"); say "the second file, from Step
5.23" in the Do, or the agent may verify the first (which reads "3 times").

### R4: S: `states.md` row `shell--no-role` still gives the former trainee's menu without Export portfolio

`states.md:118`: "At Step 5.21: no "Acting as" head; Home, then My progress and My data rights. No change." Since T379 (`NavItems.
PersonalFor`) the menu is Home, My progress, Export portfolio, My data rights, which the lane's own 5.21, A.1.6, A.4.6, A.7.4 and
the `home--no-role` row say. Fix: "Home, then My progress, Export portfolio and My data rights".

### R5: N: 3.54b and A.7.8a now meet an Export action on a registrar's page

`act-3-operations.md:1492-1518` (3.54b, Dr Zulu, Committee member) and `appendix-cross-cutting.md:1474` (A.7.8a). The registrar's page
header now offers "Export Nomsa Mahlangu's portfolio" to a CommitteeMember at the institution (`ProgrammeTraineeDetail.razor`
`HeaderActions`; `MayReadAsync` admits `CommitteeMember` at the trainee's institution). 3.54b's Expect only says "is offered no Send
a reminder" and its (old) Actual says "No button", so the next replay sees a button the Expect never mentions. Add one clause to
3.54b ("and, in the header, Export Nomsa Mahlangu's portfolio (T379)") and, in A.7.8a, that the action is a 44 px row at 390.

### R6: N: `verify-export--pasted` asks for a pasted line break in a one-line field

`states.md` row `verify-export--pasted`: "the genuine hash pasted in groups with spaces and a line break". A browser strips the line
break from text pasted into `<input type="text">`. The query does accept one (`?hash=…%0A…&check=1`). Fix: "or typed into the address with `%0A`
for the line break".

### R7: N: A.1.14's lead sentence says no Dr Ndlovu "among KGK's … trainees", and its next bullets then list his ended programme

`appendix-cross-cutting.md:282`: "No Dr Ndlovu remains among KGK's users or trainees (T026, T258)." The added bullet puts his ended
programme on Trainees' Completed and closed under `deleted_user_…`. Read together they agree, but the lead now reads as a
contradiction. Fix: "…among KGK's users or current trainees, and no row names him".

### R8: N: 5.17's user-page sentence (flow 12's) omits what the graduate can still do

`act-5-graduation.md:577-580` quotes flow 12's "They can still sign in to read their training record (My progress) and their data
rights." (it matches `UserPageText`), while the same step's account section, 5.21 and the menu now say she also exports the
portfolio and downloads certificates. A cross-flow wording, not the lane's error; file it with flow 12's owner if it is wanted.

### R9: N: `EPA-PROGRAMME.md:311` still names the portfolio-export retention as a 90-day constant

"the only 90-day constants in the repository are the portfolio-export and scheduled-job-run retention". The job is gone (migration
`T379_PortfolioExportCleanupRetired`). Outside the runbook, but the same retirement; one clause.

### R10: N: README cast "Acts" column

`README.md` (Cast table): du Plessis "2–4", Mokoena "2, 3, 4" while the runbook plays them in Act 5 (du Plessis 5.24 to 5.28) and in
the new states rows (Mokoena, Sithole). Pre-existing, but the lane's states rows lean on it.

### R11: N: 5.22's Role line and Route

`act-5-graduation.md:675-678`: the Route reads `/committee/my-reviews → /access-denied → /portfolio/authorisations → /msf/my-reports →
/access-denied → /activities/new → /access-denied`, which matches the Do's order. Nothing to fix; recorded because the Do says
"in turn" and the first of the two denials has no Route entry for the address typed in between (the same shape as before the lane).

## Held (not findings)

- 5.17 and 5.27 state "no Back to trainees (T305)" while their old Actual says "Back to trainees stays": the Actual predates the
  build and the Gap lines say the next replay re-reads the step. The active profile does keep a Cancel link beside Save profile
  (`TraineeProfileEdit.razor`), which no Expect denies.
- `states.md`'s retired row `home--trainee-ended` ("reached by no replay") is right after D54: no ended registrar holds Trainee.
- The 1.x outcome and Act 6's handoff text say "two ended programmes"; Act 6 starts with exactly two, and A.1.14 makes it three.
- `FormerTraineeRecordWords.NoneInForce` (PROPOSED) is not reachable in the scenario and no step quotes it.
