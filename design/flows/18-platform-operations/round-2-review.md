# Flow 18 round 2: the review

Round 2 (`round-2/`, 173 boards: Spec 1–2 and Steps, the page boards at 1280 with their 390 and 768 twins, two 390
boards without a base by design; `f18r2.css`) drew structure A at full fidelity: H Home, I institutions, J scheduled
jobs, R run history, L the audit log, E an audit entry.

Reviewed 2026-11-11 from three sides, each a Sonnet agent: **the code** (`round-2-review-code.md`, `844b21e1`: K1 must,
K2–K11 should, 9 nits), **the cast and words** (`round-2-review-cast.md`, `19abf3d7`: C1–C2 must, C3–C7 should, 6 nits)
and **the design system** (`round-2-review-ds.md`, `ba2f2d3a`: M1–M2 must, S1–S10 should, 7 nits). The three files are
kept as they are; this one merges them. Each must-fix below was checked by the integrator before it was carried.
Severity: M must, S should, N nit.

## What holds

- The twins match their base on text and attributes (code and cast: 88 of 88; two have no base by design).
- Q1–Q5 and Q13–Q17 hold in the boards; Q5's dialogs and commands, Q17, the built sentences quoted verbatim and every
  Spec line reference but one (K12) check out (code).
- Cast, FIXTURES, menus and lit items hold for both roles; no System item; no gendered word; the 27 Illustration chips
  are the Spec's; the Steps board's 14 of 23 changed, 9 unchanged, plus 1.23, and its states.md rows are right (cast).
- By script and rendered in Chromium at 1280/768/390: no undefined class, inline style, hex, dangling id or unlabelled
  control; every control 44px at 390; no page overflow; every contrast figure on the Spec right; f18r2.css's C1 block
  reproduces app.css's stack rules declaration for declaration (ds).

## Must-fix

| # | From | Defect | Checked | Fix |
|---|---|---|---|---|
| M1 | ds M1 | Deactivate… and Reactivate… (I13, I15 and twins) are drawn in ConfirmDialog's form mode (`dialog-form`, `dialog-actions`), not its action mode (`form-container`, `form-actions`), and Cancel has no `autofocus`. | `R2-I13`: `dialog-form` 1, `autofocus` 0; `ConfirmDialog.razor:32` form mode, `:52` action mode, `:76` Cancel's autofocus | Redraw both in the action mode, Cancel autofocused. |
| M2 | ds M2 | The pager on the R and L boards is info, page size, actions; PagerControls is info, actions, page size, and the tab order follows. | `R2-R06` vs `PagerControls.razor` | Redraw the pager in PagerControls' order and markup. |
| M3 | code K1 | Q15 does not hold by the read the Spec names: an erased account's names are blank, so `GetDisplayNamesAsync` falls back to the user id, while the row still stores the person's real name. | `IUserAdministrationService.cs:28`; `DataRightsPeople.StaffNamesAsync` (`DataRightsPeople.cs:69`) returns the pseudonym | Name `DataRightsPeople.StaffNamesAsync` (or a shared reader of it) as the audit log's name read, as a DATA row; an erased person reads as their pseudonym (E09). |
| M4 | cast C1 | Q17 also changes A.5.5 ("The institution shows Page not found", `appendix-cross-cutting.md:1003`); the Steps board lists only 1.23, and its "Page not found" search row misses A.5.5. | `appendix-cross-cutting.md:1003` | Add A.5.5 to the Steps board and the search row. |
| M5 | cast C2 | A.2.10 counts "4 + 5 (390: 5 + 5)": it adds a menu press at 390, but the journey starts on Scheduled jobs and presses Run history, a header action. | `appendix-cross-cutting.md:679`; `ScheduledJobsList.razor:12` | 390 equals 1280: 4 + 5. |

## Should-fix (carried to round 3 where drawn; to the build otherwise)

- **Build feasibility (code).** K2 Q4's count needs a new `IUserAdministrationService` method, not a join in the
  handler. K3 an in-process `IEmailHealth` resets its 24-hour window on every restart, hooks in the wrong place, has no
  "nothing sent yet" state and loses drops at shutdown (Q18). K4 "Overdue" is falsely raised by a long-running job and
  by an invalid cron. K5 "Its next run is …" can be false when a job is re-enabled. K6 Run now's result needs the
  dispatcher interface changed, a typed refusal for a disabled job, and the "unknown" runner's fallback. K7 a shared SAST
  helper and day-range conversion, and a fallback for crons it cannot word. K8 the MSF respondent row's rule keys only on
  the command's name: say so. K9 "first 48 bits" is wrong for IPv4, which is cut to /24. K10 a Failed row shows the raw
  exception's text: word it through the refusal reader. K11 the database-down partial answer needs a new DTO shape and is
  reachable only briefly. K12 `InstitutionEdit.razor:102–105` should read `:246–250`.
- **Words and steps (cast).** C3 the "dispatched" search misses 3.32, A.2.6, A.2.8 and A.2.9's Actuals. C4 four
  word-search rows name too few steps ("When (UTC)" is 3.56; "Edit institution" also hits A.6.1 and 1.23). C5 the Spec
  does not quote many NEW or PROPOSED words (subtitles, job descriptions, health sentences, helps, count and window
  lines): list them. C6 the six load-error boards and their twins carry no Illustration chip though brief § 3 says they
  are unplayed. C7 the Detail button's accessible name has no SAST zone.
- **Markup (ds).** S1 Home's two links are 21px tall at 390 outside `.dashboard-card-footer`. S2 Spec § 12 says Home is
  two columns at 768; app.css makes it one at 900px and below. S3 `.code-block--region:focus-visible` restates
  app.css:512: drop it. S4 the payload section and its region share one landmark name. S5 retiring the status dots also
  touches `ContrastTests.cs:319`, DESIGN.md:324's contrast row and `.status-dot`: list them. S6 `required=""` on 46
  controls where DESIGN.md wants `aria-required="true"`. S7 the Job column on Scheduled jobs and Run history is not
  `th scope="row"`. S8 the 1150px stack is a readability choice, not forced by overflow (natural tables overflow only
  below about 860–1020px): kept, as flows 16 and 17 chose it for their lists, and worded so on the Spec. S9 the four page
  classes lengthen the stack selector lists: acceptable. S10 the table container's `shadow` and DataTable's roles are not
  on the boards.

## Nits

Twenty-two, in the three files (code 9, cast 6, ds 7); none changes a board's meaning. Carried to round 3 as one list.

## For the operator

- **Q18, where mail health is kept (K3).** An in-process count resets at every restart, so a drop just before a restart
  vanishes and "Failing" clears itself. Recommended: persist each mail's outcome (sent or dropped, its time, the
  refusal's words; no address or body) in a small table, pruned by the existing retention job, and read the 24 hours
  from it; "Nothing sent since ‹time›" covers the empty state. The alternative is the in-process count with the line
  saying "since Wombat last started".
- **T1–T24**, the ask's provisional settlements (Q6–Q12 and words), held in round 2 and drawn as settled; no review
  disputes one. Recommended: confirm them as drawn.
