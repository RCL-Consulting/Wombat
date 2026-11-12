# Flow 18 build review: the pages and the runbook (T400)

Reviewed 2026-11-11, branch `t400` (`07920cc9`), worktree `C:\dev\Wombat\.claude\worktrees\t400`. Read-only (nothing committed, no source changed).
Paths are relative to `src/Wombat.Web/Components/Pages/` unless they start `src/`, `execution/`, `design/` or `tests/`. `AHW` = `Dashboards/AdministratorHomeWords.cs`, `JW` = `Admin/Jobs/JobWords.cs`, `AW` = `Admin/Audit/AuditWords.cs`, `IE` = `Admin/Institutions/InstitutionEdit.razor`.

**Method.** Static review, one full test run, no browser.
- Read in full: the Home page and words, the institutions list, create page, record and words, Scheduled jobs, Run history and their four words files, the audit list, entry, words and links, `SastTime`, `JobLinks`, the diffs of `app.css`, `DESIGN.md`, the runbook (six files), `ErrorPages`, `ActivityListWords`, `CatalogueLinks`, the retention job and the dispatcher, the five lane notes, `sweep.md`, and the boards' text for H01-H12, I07-I16, I22, J01, J02, J09, E02, E07 and the round-3 decisions table.
- Words: a script took every string literal of the four words files and the page razors (interpolations as wildcards, 80 characters of slack) and looked for it in the text of all round 1, 2 and 3 boards. Its misses, the lanes' lists and a hand read of the razors' literal text are the NEW list below. It lists no fragment-only match: every kept literal was found whole.
- Counts: Accounts traced against `CountAccountsAsync` (every user without an erasure record; the lock is `LockoutEnd > now`) and the old role-holder count (22 + Dr Molefe + Dr du Plessis = 24, A.6.2); the jobs line traced through `Judge` and `AdministratorHomeWords.Jobs` for 1.1, A.2.2, A.6.2; the audit and run counts are replay-dependent (166, 174, 21, 22) and were not recomputed.
- Run: `dotnet test` on `Wombat.Web.Tests` (Release, its own build): 4950 passed, 0 failed. No bUnit page of my own was needed.
- **Not done:** nothing rendered in a browser; widths and focus read from markup, CSS and `sweep.md`. The Application side was read only where a page or a runbook step quotes it (`AuditFailureText`, `CountAccountsAsync`, `Judge`, `ScheduledJobRefusal`, the retention job).

**Counts: 1 must, 2 should, 13 nit. NEW words: 26 groups** (list at the end).

## Must

### M1. Step A.2.9 expects a log line the retention job no longer writes
- `execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md:680` expects `scheduled-job-run-retention: "no old runs to delete."`. Lane B changed the line: `src/Wombat.Infrastructure/Scheduling/Jobs/ScheduledJobRunRetentionJob.cs:49` now logs "ScheduledJobRunRetentionJob: no old runs or mail outcomes to delete." (the job also deletes mail outcomes, Q18). The step is not among lane W's changed steps, and `design/flows/18-platform-operations.md:920` carries the old words too.
- Verified by reading both lines. A replayer comparing the log to the Expect records a false Gap. Fix: change :680 (and the Actual's quote at :685 keeps its date, "predates T400"). The step's wording "Each run reads Succeeded" is right.

## Should

### S1. `states.md:145`: Home with msf-campaign-auto-close disabled says "The other 7 last succeeded."
- The row is captured "At Step A.2.2, before enabling". At that step weekly-coordinator-digest has never run (A.2.1's Actual; A.2.4 runs it). `AHW.Jobs` (`AdministratorHomeWords.cs:128-139`) counts Succeeded and Running only (6) and appends the never-run job's sentence, so Home reads "msf-campaign-auto-close is disabled, so the scheduler does not run it. The other 6 last succeeded. weekly-coordinator-digest has not run yet: its first run is ‹day› 10:00 SAST." The board (R3-H06) draws 7 because all eight have run there.
- Fix: say 6 and the digest sentence, or move the capture after A.2.4.

### S2. DESIGN.md records Home, the institutions and the audit log, but not Scheduled jobs or Run history as pages
- The flow 18 text holds the Run now unavailable rule (:781), the stacks and the column shares (:1171-1183), the pager bullet (:3392) and the audit log and entry (whole). Nothing records the two job pages: the schedule in words in SAST over `cron ‹expr›, UTC` (`ScheduleWords`), Last run (day and hour `.nowrap`, badge, a failed run's error under it) and Next run ("None while disabled", "At the next check, within 30 seconds", "None: the schedule cannot be read"), the result region above the list, the Run history filter (Job a select of the keys, Status, From and To as South African days, Show writing the page's own address, Clear filters only once a filter is applied, the `h2` "Runs", the count line that Show focuses, "No job has run yet" with no count line and no pager), and `?job=&status=&from=&to=&page=&size=` as the address Home's "Its runs" opens.
- Flow 17's review (S4) asked for the same of its list. Fix: two short paragraphs under Page-level patterns.

## Nit

- **N1. DESIGN.md's institutions bullet says Edit is "named for its row by a visually hidden name";** the build uses `aria-label="Edit ‹name›"` (`InstitutionsList.razor:93`) because `RowActionMarkupTests` holds every row action to an aria-label. Accessible name is the same and contains the visible word. Edit the sentence.
- **N2. Sweep fault 1 (payload region `content-box`): fine.** The Spec quotes the declaration `max-height: 28rem` (round-3 boards-text:206), and the build is that declaration literally. Say "28rem of content" in DESIGN.md:3940 so the next reader does not measure 30rem.
- **N3. Sweep fault 2 (Disable acts at once): fine.** R3-J02 draws no dialog (board text :4743-4824: the result "Disabled …" above the list, no dialog), the decision table's Q10 says results above the list, and Disable is reversible by Enable.
- **N4. Sweep fault 3 (two command names break once; `msf-invitation-expiry-reminder` takes two lines): fine.** Round 3 accepted devadmin's Action at 237px on two lines and R3-R07's Job on two.
- **N5. Sweep fault 4 (Run history at 1025 with a long error: Status 215px, 15 lines): fine, with a note.** The error whole is the row's point (T1, R3-R07), and 1025 is the narrowest table. A 2,000-character error would be about 90 lines at that width; the sweep did not measure one. If the next replay finds the page unreadable, widen the 1024px stack for Run history, not the share.
- **N6. Lane W's open points: all fine.** (a) The Administrator's Unavailable trail is "Home › Institutions › Institution unavailable" by the owner table (`Navigation/NavOwners.cs:147`) and `CurrentCrumb`; the test (`InstitutionEditRecordTests.cs:210-221`) pins the h1 and the link but not the trail; low risk. (b) 1.1's digest line: the note's rule ("a Monday after 10:00 SAST or a Tuesday before it") is right for `0 8 * * 1` UTC. (c) A.6.2's heading still fits its Do.
- **N7. Home's jobs line on odd data.** With no job definitions it reads "All 0 jobs last succeeded." on an OK badge (`AHW.Jobs`, `needing.Count == 0`, `neverRun.Count == 0`), and a job mid-run counts as "last succeeded" (lane A's departure). Neither occurs in a replay (the host seeds the eight before Home loads); a one-line guard for zero would be honest.
- **N8. A page past the last** (`?page=99`, by hand) on Run history draws "No runs match" with no pager and the card's Clear the filters (`ScheduledJobRunsList.razor:112-124`), on the audit log "No actions match" with no Clear when unfiltered but the pager drawn (`AuditList.razor:150-158`). Only a hand-made address reaches it; both recover.
- **N9. Native `disabled` on Deactivate… and Reactivate… while a save is in flight, and on Save while a status change is** (`IE:142, 158, 163`). DESIGN.md accepts the T234 in-flight race (the focus is on the pressed button, not these). Not an unavailable action in the Run now sense.
- **N10. The audit log's subtitle for an institution's administrator appears after the first read** (`AuditList.razor` `Subtitle`, lane C); R2-L14 draws it while loading. The name is not known before the read. Accepted.
- **N11. `/admin/institutions/abc` and an id past `int` are Page not found,** not Institution unavailable: the route is `{Id:int}`. A.5.5's "an unknown id reads the same" holds for numeric ids, which is all a person types.
- **N12. Two builders of the same address and one date helper.** `AdministratorDashboard.razor` `ItsRunsHref` and `JW.FailedRunsOf` both call `JobLinks.RunsOf(key, "Failed")`; `AW.Millisecond` does its own format through `SastTime.ToSastInput` (lane C suggests `SastTime.Millisecond`). Cosmetic.
- **N13. `FormFieldHelpTextLinkTests` guard lowered 10 to 9 files and 80 to 75 fields** because the two institution pages hand-write their fields (a refusal must come first in `aria-describedby`, T378). Accepted: `InstitutionCreatePageTests` and `InstitutionEditRecordTests` pin the order. Also Step 3.32 (unchanged) does not name the new result "Ran assessor-pending-nudge: Succeeded at …"; its Expect needs none.

## The departures the integrator accepted (judged)

| Departure | Verdict |
|---|---|
| Lane A: create and record fields hand-written | Right, N13. |
| Lane A: Edit named by aria-label | Right; DESIGN.md says otherwise, N1. |
| Lane A: the institution's own administrator's view keeps flow 12's form, bold first sentence, `SaveFailed` for any non-refusal | Right: the two changes the brief names (Q17's Unavailable with Go to Home; the failure words fixed, T329). Her trail has no Institutions crumb, as before. |
| Lane A: Active/Inactive via `BadgeFor.ProgrammeOutcome` | Right: Active green, Inactive `.badge-withdrawn` (DESIGN.md says the same). |
| Lane A: a Running last run counts as succeeded; failed beats overdue | Fine, N7; failed beats overdue is the louder, correct word. |
| Lane B: row actions by aria-label ("Run now: ‹key›", "Disable ‹key›") | Right: each contains its visible word; "Running…: ‹key›" is the board's own (J05). |
| Lane B: run history's address carries `size` when not 50; Show writes the address | Right: it is what makes Home's "Its runs" and the way back return to the same rows. |
| Lane B: "No job has run yet" whenever no run is stored | Right: with 90-day retention it can only be a new install. |
| Lane B: ActionFocusTests job scenarios `CanBeRefused` false | Right: a refusal now takes the focus in the result region (N7), so the scenario no longer applies. |
| Lane C: the window moment in the address (`opened=`/`shown=`), From and To as typed SAST | Right and needed: without it a page turn or the trail back would read a later window. |
| Lane C: the trail adds `entry=` and the list focuses `#detail-‹id›`; `SafeLogReturn` allow-lists the parameters | Right (R3-L13). `entry` is not itself accepted in `back`, so a crafted address falls back to the plain log. |
| Lane C: pager drawn whenever a row; "Clear the filters" only while filtered; Clear keeps the page size | Right. |
| Lane C: K10 as P1 decided (a refusal is Wombat-thrown); two-parameter pipeline constructor with a no-op logger | Right in the pages' terms: Step 3.55-3.56 read the refusals' own words (`AuditFailureText.cs`). The constructor is Application scope. |

## Checks that came out clean

- **Home** (`AdministratorDashboard.razor`, `AHW`): three lines in fixed order, name, hidden ":", badge (OK green; Failing, Unreachable red; Not configured, Disabled amber; Overdue `.badge-overdue`; Not read grey), sentences in words; no dot, no link in a sentence; footers: "Its runs" only when exactly one job failed (named "Its runs: ‹key›, failed"), then Scheduled jobs, none when the jobs line reads Not read; Accounts "N" over "account(s)", the sentence, the locked line, Users, Not read with no footer. Every sentence checked against H01-H12: identical.
- **Institutions** (list, create, record): subtitles, help lines, the mark line, `aria-required` and the visually hidden "required", no `required`/`maxlength`, the refusal first in `aria-describedby`, the first refused field focused, a duplicate under Short code (`CreateInstitutionCommandHandler.cs:48-55` names the field), Save lands on `?created`, "‹name› created." focused, token replaced; the Status section (h2, badge, sentence, the one trigger the state allows); ConfirmDialog action mode, Cancel first and focused, Deactivate danger, Reactivate primary; results by name; K17 (typing kept); Q17 for both roles with the right link.
- **Scheduled jobs:** result region above the list and focused after Done, Refused (warning, `role="alert"`), Run failed (danger, "Its runs") and Failed (T23 words, first sentence bold); Run now `aria-disabled` with its reason as a `span` and `aria-describedby`, a press sends nothing; "Running…" with the other row actions natively disabled (T234); the dispatcher refuses a disabled job under the lock; the page reads again after each action. Times are SAST with the zone in every cell.
- **Run history:** Job a select with "All jobs" first, Status, From and To as South African days, Show (aria-disabled until the first read), Clear filters only when applied; the address is the state; the `h2` "Runs" takes a page turn, the count line takes Show, Job takes Clear; PagerControls in its order (info, Previous and Next, Per page); the error whole under its badge; Ran by a name or "Scheduler".
- **Audit log and entry:** filters in the boards' order, Category by label, Institution for the Administrator only, nothing filters until Show; the window fixed, said and carried; the trail returns to the same rows with the Detail focused; Detail named "Detail: ‹action› at ‹time› SAST" (made distinct by `RowNames`); the entry's four sections by `h2`, the payload region `tabindex="0"`, `role="region"`, `aria-labelledby` its h2 and the section unnamed (S4), the Administrator's alone; "Audit entry unavailable" never says which.
- **Rules:** no inline `style`, no hex outside `:root` (the CSS diff adds none), no `bi bi-`, no exception text on any page (the only `.Message` uses are typed refusals), no `disabled` on an unavailable action (Run now, Previous and Next, Show are aria-disabled), `col-fit--wide` and `.person-cell` reused, `.status-dot` and its contrast row removed together.
- **CSS:** the three Platform operations sections hold what DESIGN.md says (`.health-*`, the two error shares, `.code-block--region`, Action `width: auto`, Result 20%); the four lists join the seven selector lists (C1).

## Runbook against the build

Every changed step's Expect checked against the code:
- **1.1, 1.11, A.6.2:** the three lines and their words (`AHW`); Accounts 9, 11, 24 trace (the old count plus the two roleless graduates, less the erased). A.6.2's "All 8 jobs last succeeded." holds after A.2.4. The 1.1 digest line and its weekday rule: right.
- **1.6, A.6.1, 1.23, A.5.5:** every word and the focus as built; "created today, a day with no time" is `SastTime.Day`; the Unavailable pages and trails as above.
- **3.55-3.57, 4.38:** subtitle by role, "Show moves the focus to the count line", the window line in its "when you pressed Show" form (`AW.WindowLine`), the Institution filter's options and order, "MSF respondent (no account)" over "No institution", "No person: an MSF respondent answers by link, with no account.", "Cut to its first 48 bits when stored.", the trail's return with the Detail focused, no Payload for Prof Mbatha. The counts 166, 174, 5, 1, 4 and 19 are replay data.
- **A.2.1, A.2.2, A.2.4, A.2.10, A.7.11:** columns, schedule words, "None while disabled", "Enable it to run it.", "Enabled … Its next run is …" and the K5 twin, "Ran … Succeeded at … SAST." above the list and focused; Run history's filters, "2 runs.", "No runs match these filters." then the card; "Scheduler". **A.2.9 is M1.**
- **`states.md`:** rows 141-1101 read against the pages; **S1** is the one wrong word. The rest of the "Not yet captured" rows quote words the code renders.

## Words on no board (NEW), 26 groups, each beside what it replaces (C5)

Lane A, Home (`AHW`):
1. "1 mail was dropped in the last 24 hours, the last at ‹time›…" (n = 1). Replaces R3-H04's "‹n› mails were dropped …" at n = 1.
2. "1 is locked and cannot sign in now." Replaces R3-H10's "‹n› are locked …" at n = 1.
3. "‹n› waiting" in the Mail sentences. Replaces "none waiting" (R3-H03, H12) when the queue holds mail.
4. "‹key› has not run since it was due: the scheduler did not start it. The application log says why; Run now runs it." (no time). Replaces R3-H07's "… due at ‹time› SAST: …" for a job with no next occurrence.
5. "‹key› has not run yet." Replaces R3-H02's "… has not run yet: its first run is ‹time› SAST." when no first run is known.
6. "Two / Four … Ten jobs need you:" (a number in numerals above ten). Replaces R3-H09's "Three jobs need you:".
7. "‹n› job last succeeded." and "All 1 job last succeeded." Replaces R3-H01/H02's "7 jobs …" / "All 8 jobs …" at n = 1.
8. "account" (the singular label). Replaces "accounts" (R3-H01) at 1.

Lane A, institutions (`InstitutionWords`, `IE`):
9. "No institutions yet" and "Create the first institution to start configuring scope and onboarding." (kept from master; no board draws an empty list). Replaces nothing.
10. "Could not deactivate the institution. Nothing has changed. Try again, or come back in a few minutes." Replaces nothing; the twin of R3-I22 for the Status section.
11. "Could not reactivate the institution. …" As 10.
12. "Could not save the institution. …" on the create page (R3-I22 draws it on the record). Replaces master's create-page failure text.

Lane B (`JW`):
13. "None: the schedule cannot be read" (Next run, an unreadable cron). Replaces "—" (K4's note; the Home sentence is § 5 row 2).
14. "Enabled ‹key›." (no next run known). Replaces R3-J03's "Enabled ‹key›. Its next run is ‹time› SAST."
15. "‹key› could not be enabled." and "‹key› could not be started." Replace nothing; twins of R3-J10's "could not be disabled".
16. "The filters are kept above. Clear them to see the one run." Replaces R3-R03's "… see all ‹n›." at n = 1.
17. Durations "1.2 s" and "2 min 5 s" (R3-R07 draws "7 ms"). Replace the old page's Finished column.
18. "Unknown" (a status that is none of the three). Replaces an enum name read through.
19. "2,800 runs." (a thousands separator). Extends R3-R01's "21 runs.".
20. "Run now needs a signed-in person." (audit text only, never on a page). Replaces nothing.

Lane C (`AW`):
21. "Read from ‹From› SAST to ‹To› SAST. Turning a page keeps this window." (a To typed). Replaces R3-L01's "… to ‹hh:mm› SAST, when you pressed Show. …".
22. "Read from ‹From› SAST to ‹hh:mm› SAST, when you opened the page. …" (a From typed, the address opened). Replaces R3-L's "… : the last 24 hours, when you opened the page. …".
23. " by ‹person›" in a Detail's name when two rows share action and second. Replaces nothing; extends C7's "Login at ‹time›".
24. "None" for a missing address, subject type or subject id (R3-E02 draws it for User id only). Replaces "—" (master).
25. The category "Other" (a value the label map does not know). Replaces an enum name.
26. A.2.2/A.6.2-style singulars are not repeated here; the remaining group is the entry's subtitle for an actor with no name ("‹action› by No signed-in person, …"), which composes two board phrases. Replaces nothing.

Not NEW though the lanes listed them: "Set by a Wombat administrator.", "Save changes" and " It has no contact email now." are flow 12's, on master and in its boards.
