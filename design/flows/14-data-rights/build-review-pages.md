# T387 build review: the pages, the design system and accessibility (flow 14)

Reviewer: read-only, by reading markup, C# and CSS (no app run, no test run, no browser; every measurement is from CSS and
markup, and a claim about a browser's behaviour is marked inferred). Branch `t387` at `71190f07`, build `c47bc6e0..71190f07`.
Checked against `round-3/boards-text.md` (the Spec boards and the M, Q, D, C, K, U boards at 1280), `round-3/f14r3.css`,
`round-3-check.md` (Build notes), `design/flows/14-data-rights.md` § 6, the operator's approval at the top of
`build-lanes.md`, CLAUDE.md and DESIGN.md. Read: `DataRights.razor`, `MyDataRightsWords.cs`, `RequestsList.razor`,
`RequestDetail.razor`, `DataRightsRequestWords.cs`, `DataRightsWords.cs`, `DataTable.razor` (`Class`), `BadgeFor`, the 140 new
`app.css` lines, `ConfirmDialog`, `PagerControls`, `StatePanel`, `ActionResult`, the DESIGN.md diff. The code review
(`build-review-code.md`) is not repeated.

Result: **1 must-fix, 4 should-fix, 6 nits.** The words match the boards almost everywhere (every difference is listed under
Words); the pages follow the system (one h1, titles, tokens only, no inline style, `Icon`, `IScopedSender`, no exception text,
SAST, refusals under fields, results focused). The must-fix is the DESIGN.md banner; the should-fixes are a list that
reloads itself into a skeleton, DESIGN.md amendments the Spec asked for and the build did not write, a wrong refusal for a
long note, and a dialog whose first focus is not guaranteed to be Cancel.

## Findings

### P1 (must-fix): DESIGN.md's amendment banner does not list flow 14
- `execution/architecture/DESIGN.md:~20-62` ends its list at "flow 13, graduation and exit (2026-10-27, T379 ...)". The banner is
  where each flow adds itself "with its date and commit"; flow 14 (2026-10-30, T387) is absent, though the body carries
  seven flow 14 amendments (`:763`, `:1081`, `:1876`, `:1967`, `:4136`, `:5100-5117`).
- Fix: add "flow 14, data rights (2026-10-30, T387: § Button system (an erasure is approved through its dialog), § Table system
  (`DataTable`'s `Class`, the queue stacks from 1100px), § Badges (DataRightsRequest), § Page-level patterns (Unavailable, My
  data rights, the queue and a request), § Form system, § Alerts, and § app.css section order (three Data rights sections))",
  with the commit when it exists (flow 13's own entry still has none: its review P9).

### P2 (should-fix): every Show, Clear filters, page turn and page-size change puts the queue back into its loading skeleton
- `RequestsList.razor:196-200` (`LoadAsync` sets `_loading = true` unconditionally) with `StatePanel IsLoading` (`:30`), which draws
  `LoadingContent` instead of `ChildContent`: the filter form, the table and the pager are removed and rebuilt on each read.
- Evidence it is unintended: the comment at `:275` ("a change of size leaves the focus on the select") is false, since the
  select is destroyed; the typed-but-unapplied filter is dropped (`SetFields`, `:237`); the page flashes "Loading the data rights
  requests." (a `role="status"` line) on every press; and Users does the opposite on purpose
  (`UsersList.razor:285-300`, `showLoading: !_loadedOnce`: "Show and a page turn answer in place").
- Effect: a keyboard user who changes "Requests per page" loses the focus to the page body (WCAG 2.4.3); Previous/Next are
  rebuilt mid-press; R3-Q10's "focus on Requests (50)" works only because the heading is re-created afterwards.
- Fix: `LoadAsync(bool showLoading)`, true for the first read and Try again only, as Users does.

### P3 (should-fix): the DESIGN.md amendments Spec 4 and build note 7 name are mostly not written
- Written: § Button system (the erasure dialog, `:763`), `DataTable` `Class` and the 1100px stack (`:1081`), Unavailable (`:1876`),
  Badges (`:1967`), a full My data rights section (`:4136`), the section order (`:5100-5117`).
- Not written (round-3-check.md note 7; R3-0-spec-4 "DESIGN.md text each change touches"):
  - § Form page (`:1277`) and § Form system's counter line (`:1435`): N17, the refusal first in `aria-describedby`, then the
    help, then the counter. Only the My data rights section says it.
  - § Table system: the Answer column, the Request id column, `th scope="row"` with `DataTable`'s explicit roles, `data-label`
    on every stacked text cell (M5, M6), the Administrator's Institution column, the pager only past one page (F10), captions with
    the filter's tail.
  - § Detail page and the title rule: a request is "‹Type› request from ‹live name›" (F1), one column of h2 sections on
    `.user-page`, Decided by the name over the id, the pseudonym `.person-cell` (N13). The queue and the request page have no section
    of their own, as My data rights has.
  - § Alerts: the standing erased notice (`Role=""`), and the live requester name in every result.
  - `:3426` still says the request's crumb is its id "once it has loaded"; the page draws the id from the first render (N14).
- Fix: write them; DESIGN.md is "mandatory before any Razor work" and the next flow's planner reads it.

### P4 (should-fix): a decision note over 4,000 characters is refused as "A decision note is required."
- `RequestDetail.razor:702-706` maps any `ValidationException` on `DecisionNote` to `NoteRequired`; the validators also hold
  `MaximumLength(4000)` (`ApproveDataRightsRequest.cs:35`, `RejectDataRightsRequest.cs:31`). A pasted 4,001-character note is
  refused under the note with a sentence that says it is missing. The page has no counter and no length check before the send
  (My data rights has both, S24), and D19 draws a "long note".
- Fix: check `_note.Trim().Length > 4000` in `NoteGivenAsync` with "Give your note in 4,000 characters or fewer." (the Reason's
  sentence, `MyDataRightsWords.ReasonTooLong`), and map the validator's refusal by length, as `RefusedField` does on My data rights.

### P5 (should-fix): the erasure dialog opens on Cancel only if the browser says so (inferred)
- R3-D07 and DESIGN.md `:763` say the dialog "opens on Cancel". `ConfirmDialog`'s action mode puts no `autofocus` on Cancel
  (`ConfirmDialog.razor:78`; the form mode does, `:48`), so the native first focus is the first focusable area in tree order.
  The body is now `div.dialog-body--scroll` (`max-height: 55vh; overflow-y: auto`, `app.css:~7262`). In Chromium a scroll
  container with no focusable child is itself keyboard-focusable, and the dialog's focus steps can pick it before Cancel when the
  forecast is long (a phone, a staff member with reviews, campaigns and requests). Not run.
- Fix: `autofocus` on Cancel in the action mode (harmless to every other dialog, which already opens there), or `InitialFocusId`
  with an id on Cancel; either way a test that the focus call names Cancel for a long forecast (the runbook step A.1.12 and
  state `erasure-dialog` expect Cancel).

### P6 (nit): refusals on the request page are not dropped when the field changes, and the record radios are not `aria-invalid`
- My data rights drops its reason refusal on change (`DataRights.razor:290`, DESIGN.md Form system). `RequestDetail` keeps
  `_noteError`, `_choiceError`, `_otherNameError`, `_beforeError`, `_afterError` until the next press (`:532-545`, `:761-786`;
  `Choose` clears only `_choiceError`). The My data rights section says a refused radio group's radios are `aria-invalid`; the Record
  fieldset (`:393-431`) sets `is-invalid` and `aria-describedby` on the fieldset only.
- Fix: clear each on its field's change; `aria-invalid="true"` on the radios while `_choiceError` is set.

### P7 (nit): a stacked row's header keeps a bottom border
- `app.css` `.data-rights-table tbody th[scope="row"]` (specificity 0,2,2) sets `border-bottom: 1px solid`, and beats the stack
  rule `.clinic-table--stack-wide tbody th { border: 0 }` (0,1,2) at 900px and below on Your requests and Corrections recorded, so
  a rule is drawn under the date or the record inside every stacked card, above the labelled cells. The queue is spared only because
  its own 1100px block, later in the file, restores `border: 0` at equal specificity. The boards' `f14r3.css` has the same rule
  (so it is the design's), and the stack-wide tables elsewhere draw no such line.
- Fix: put `border-bottom` in a `min-width: 901px` block, or repeat `border: 0` for the stacked state.

### P8 (nit): two implementations of "to the second", two of the same-minute naming
- `MyDataRightsWords.ToTheSecond` (`:223`, adds two hours by hand) and `DataRightsRequestWords.WhenToTheSecond` (`:151`,
  `DateTimeOffset`), and `Made`/`ReviewNames` repeat one algorithm. `DataRightsWords.When` already delegates to
  `ActivityMoments`. Fix: one `DataRightsWords.WhenToTheSecond`, one grouping helper.

### P9 (nit): the page words class reads an Infrastructure constant
- `DataRightsRequestWords.cs:~332` shows `Wombat.Infrastructure.DataRights.ErasureExecutor.PersonChanged`, the executor's
  exception sentence, on D11. The text is the board's, so no exception text reaches the page in the T329 sense, but Web words now
  depend on an Infrastructure class and on an exception message staying a user sentence. Fix: put the sentence in Application beside
  `DataRightsRefusal` and let the executor use it.

### P10 (nit): the inline "Open ‹name›'s programme" link is 20px at 390 and DESIGN.md does not name the exception
- `RequestDetail.razor:404-405`, a link inside `p.choice-help`. WCAG 2.5.8's inline exemption applies; flow 13 named its twin
  (its review P10). Add one line to the request page's section. `.request-id`'s `word-break: break-all` also lets a 36-character
  id break in the middle of a desktop column; the design's, but `overflow-wrap: anywhere` alone would keep it whole when it fits.

### P11 (nit): the request page does not re-read when `Id` changes
- `RequestDetail.razor:161` reads in `OnInitializedAsync` only. Nothing links one request to another today, so it is unreachable;
  add `OnParametersSetAsync` on `Id` before the first link between requests.

## Known departures, judged

| Departure | Verdict |
|---|---|
| The queue stacks from 1100px | Accepted. Q23 asks for no split word from 901 to 1100; the rules repeat the 900px block under `.data-rights-requests`, the row buttons 44px, and DESIGN.md `:1081` records it. Cost: a 1101px viewport has about 850px of main column for six or seven columns with a 36-character id; the id breaks anywhere, so it fits. |
| Show before Clear filters in the DOM | Accepted, and better than the lanes' rule. The bar is a row (`.filter-actions`), not a `column-reverse` stack; at 390 each button is 100% wide, so Show is on top (R3-Q02/03 at 390 draw that) and the tab order is Show then Clear. The lanes' "Clear before Show" would put the secondary first in tab order for no visual gain. |
| Load-error lead not bold | Accepted. R3-M15, Q12 and D18 draw the sentence plain; the build does too. |
| The crumb id not monospace | Accepted. `Breadcrumbs` draws every crumb in body text at 600 (DESIGN.md `:3426`); R3-D17 draws it plain. |
| "Request unavailable" keeps Data rights requests lit | Accepted (Q17). `NavOwners.cs:139` maps `RequestDetail`, and the page draws in its own place; DESIGN.md `:1876` says so. |
| No type preselected on My data rights | Accepted. R3-M01 to M18 draw no radio checked, and DESIGN.md says "none chosen". It makes `TypeRequired` ("Choose the request type.") necessary, a word no board draws (below). |

## Checks that passed

- **System rules.** One h1 per page (`PageHeader`); titles "My data rights · Wombat", "Data rights requests · Wombat", "‹Type› request from ‹name› · Wombat" and "Request unavailable · Wombat"; the owner table lights the queue for `RequestDetail`; breadcrumbs Home › Data rights requests › id; no `style=`, no `<style>`, no raw hex in the 140 new CSS lines (tokens and the spacing scale only); `Icon` only (`download`, `refresh-cw`); no `bi`; no exception text (`rg "\.Message"` finds none in the three pages); no Domain `using` in the three `.razor` files beyond `_Imports`' existing; `IScopedSender` on all three.
- **ConfirmDialog.** Cancel first in the DOM, the danger "Erase and approve" confirms, the trigger is an outline "Approve and erase…" with `aria-haspopup="dialog"` and `ReturnFocusId`; the note is checked before the forecast is read and the dialog opens; the dialog closes before the result takes the focus; Withdraw has no dialog (Q14).
- **Refusals and focus.** My data rights: the type under its legend and the reason under its field, `aria-invalid`, the first refused field focused, typed values kept (M02), one result at a time (M13), each result an `ActionResult` that takes the focus, no h2 with a tabindex. The request page: the note (D02) and every correction field (C04) under their fields, focus in the first, a command's refusal in its section's result, the note and values kept (D10, D11, D12, D16, D21, C10); Complete's refusal beside its button with the focus kept (C03); a failed load's Try again focuses the h1.
- **44px at 390.** The filter selects and the pager's page-size select (`app.css` system rule); Show and Clear filters 100% wide and 44px; the row buttons (`.clinic-table--stack-wide .btn`, 44px at 900px and below, and the data-rights page's full width); `.user-page` makes every control, radio label and stacked action 44px at 640px, the reminder checkbox's label by its own rule. Exception: the inline programme link (P10).
- **Tables.** `DataTable` with `StackWide`, `Class="data-rights-table"` after the system's classes (the literal `clinic-table` kept for the scans), explicit roles, `th scope="row" role="rowheader"`, `data-label` on every text cell (Status's badge cell included), none on the row header and the actions cell; captions as the boards (the filter's tail); one action a row named for its row (`RowNames.Distinct`); the pager only when `TotalCount > PageSize`.
- **Times and ids.** Every moment through `DataRightsWords.When` (SAST, to the minute) or `DayOf`; the second only to split a same-minute pair; ids in `.request-id` on the queue and the request, never on My data rights.
- **Badges.** Submitted blue, Approved amber (`badge-accepted`), Completed green, Rejected red, Withdrawn `badge-withdrawn`, UnderReview gone.

## Words: every difference from the boards

Compared sentence by sentence with R3-M01 to M18, Q01 to Q13, D01 to D22, C01 to C11, K01, K02, U01, U02 (1280).

| Where | Board | Build | Verdict |
|---|---|---|---|
| Reminder emails' help, every M board | "...such as an invitation to give feedback or a coordinator's reminder about one activity." | "...such as an invitation to give feedback, a coordinator's reminder about one activity, or a notice that an entrustment decision is about to expire or has expired." | Right. Round 2's F7 decided "the build names it in that sentence's examples when it checks the list against the senders"; `MyDataRightsWords` records the check (2026-10-30). The boards (M01 to M18, including M08 and M09) predate it, so the baseline will differ by that clause. |
| Answer, a note that does not end a sentence | M04, M09 draw the note ending its own sentence: `“…data subject.”` | adds "." after the closing quote only when the note ends no sentence | Right, and the lanes' text (`“‹note›”.`) is the case the code handles; M08's `”.` is a bracket. |
| Everything else on M01 to M18, Q01 to Q13, D01 to D22 (the dialog's lines included), C01 to C11, K01, K02, U01, U02 | | | Identical, word for word (checked). |

Words the build ships that no board draws (for `build-contracts.md` § 6, whose table the lanes did not fill):

| Words | Where | Judgement |
|---|---|---|
| "Choose the request type." | `MyDataRightsWords.TypeRequired` | Needed by "none chosen". Keep; not in the operator's list, so list it. |
| "Give your reason in 4,000 characters or fewer." | `ReasonTooLong` | Keep (the appeal lodge's words); the page also refuses it. |
| "The request could not be withdrawn just now. Nothing has changed. ..." and "The reminder emails could not be saved just now. ..." | `WithdrawFailed`, `SaveFailed` | Keep: M16's shape, T329. |
| "This request was decided before it could be withdrawn. Nothing was withdrawn." | `DecidedBeforeWithdrawn` | Approved by the operator (top of `build-lanes.md`). |
| "Nobody has made a data rights request yet." / "Nobody at ‹institution› has made a data rights request yet." | `NoneBody` | Keep; no board draws an empty unfiltered queue but S4's. |
| "This request is no longer waiting for a correction. Nothing was recorded." | `NoLongerApproved` | Keep; the page re-reads. |
| "What the erasure would end could not be read just now. ..." | `ForecastFailed` | Keep. |
| "The request could not be completed just now. ..." | `CompleteFailed` | Keep: C10's shape. |
| Dialog singulars: "The role ‹X› and its scope.", "1 MSF campaign not yet released: ‹n›.", "1 other open data rights request: ... It is withdrawn.", "‹n› programmes ended today", and the singular "1 open committee review ... It is withdrawn" | `DialogEndsLines`, `Erased` | Keep: the plural rule ("Every count has its singular"). The one-review line is D07's own words. |
| "This person's personal data was erased on ‹day›. The request is kept under the pseudonym." | `ErasedNotice` for a non-Erasure request | Approved class (F3); K02's rows need it. |
| "This request was withdrawn before it was decided. There is nothing to decide." | `WithdrawnLine`, erased person | Keep (K02's Access): the erasure withdrew it, so the person did not. |
| Decide's help for Access and Rectification; the Access and default result sentences | `DecideHelp`, `Approved` | Keep: they follow D01 and C01. |
| "You cannot decide your own request: another Coordinator, or the Wombat administrator, decides it." | `OwnRequestLine`, no institution | Keep (S4's neighbour). |

## Not checked (needs a browser)

Focus order and the first focus of the dialog (P5), the 390 measurements of the controls, the 1101px queue, forced-colours, and
the screenshots against the boards. These belong to the replay of the runbook's steps.
