# WaitingForAssessors

Waiting for assessors (`/programme/waiting`, `Pages/Programme/WaitingForAssessors.razor`): the requests in the programme of the role the page reads as whose next move names one assessor, supervisor or reviewer, oldest first, each with whom it waits, its state, how long it has waited and Send a reminder. Flow 06 (T358, 2026-10-05, 85d5a508, with 94edf2b7; R2-Waiting w1–w15; Q3, Q4, C3, C4, C11; E3, E4, E6) made it, in the place of the Stalled activities stub flow 01 removed. It admits both speciality admins and the Coordinator, and is an item of each one's menu; not the Committee member's, whose work is not chasing.

## The page

- **`PageHeader`**: "Waiting for assessors", with the scope under it once read: "Requests at Kgosi Kgari Teaching Hospital whose next move names an assessor, supervisor or reviewer, read as Coordinator. Your own requests are not listed." ("in Paediatrics …, read as Speciality admin" for a speciality's scope). No trail: the list lights itself.
- **What it lists** (E3, E4): a request whose pinned workflow says it waits for one named person (a `field:` nominee: a Mini-CEX in `requested`, a portfolio review in `submitted`); never work a role holds, and never the reader's own. Read as the page's role (`ProgrammeReadAs`), never the union of the roles held. The one read Home's card sends (`ListWaitingForAssessorsQuery`), so the card and the page agree.
- **The status**, `p.visually-hidden[role="status"]`: "Loading Waiting for assessors." while it reads.
- **The filters**, applied with Show (FilterBar): Waiting (`select#f-show`, `name="show"`: All, Overdue only) and With (`select#f-with`, `name="with"`: Anyone, then each waiting row's nominee by surname); Show; Clear filters once a filter is set. A GET form named "Filter Waiting for assessors", its fields carrying the query's keys (`?show=overdue&with=<id>`), so Show works before the circuit starts. Drawn while it reads or has failed, and whenever something waits; with nothing waiting, no form.
- **The answer to a reminder**, in an `ActionResult` above the list, always drawn so it can take the focus (ReminderAction).
- **The list**, `section.list-section` named by `h2#waiting-h.list-section-title` (`tabindex="-1"`), which takes the focus after Show and after a page turn: the heading, the rule line, the table and `PagerControls` (20 a page, "Waiting, pages").

## The heading and the rule line

- **The heading counts the answer** in flow 04's words (`WaitingWords.StaffCount`): "3 waiting, 2 overdue"; with an assessor asked, "1 waiting, 1 overdue, with Mohammed Patel"; no match, "0 of 2 waiting"; nothing waiting, and while it loads, "Waiting".
- **The rule line** (`p.needs-you-rule`, `WaitingWords.PageRuleLine`): "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5. Waiting counts from the last move: any save restarts it.", the two numbers the settings' (`AssessorDueDays`, `AssessorNudgeDays`).

## The table

`table.clinic-table.clinic-table--stack.clinic-table--stack-wide` in a `.table-container.shadow`, its caption visually hidden ("Requests waiting for a named assessor, oldest first"; "Requests waiting for Mohammed Patel, oldest first"):

| Column | Cell |
|---|---|
| Activity | `ActivityLink` with `FromSubject` and `Block`: "Mini-CEX (Paediatrics) · PAED-004 · 2026-10-02" over "from Nomsa Mahlangu"; two that read the same are told apart in their accessible names (`ActivityRowNames.Waiting`, C11). No `data-label`. |
| With | The nominee by name, "Thandi Zulu". |
| State | `span.needs-you-badges`: the state's badge ("Requested", "Awaiting review"), and "Overdue" beside it, never in its place. |
| Waiting | `td.waited-cell`: "8 days" (bold) over "since 2026-09-27 15:04 SAST" ("Less than a day" under a day); then, for a role that may remind, ReminderAction: "Reminded 2026-10-05 by Pieter Smit", and Send a reminder or why not. |

It stacks at 900px and below (`.clinic-table--stack-wide`, A.7.8a), every cell but Activity labelled.

## Empty, no match, loading, failed

- **Nothing waits:** under "Waiting", the empty card "Nothing is waiting" · "Nothing at Kgosi Kgari Teaching Hospital is waiting for an assessor, supervisor or reviewer.", and no form. A role with no programme to read gets the same card with "Nothing is waiting for an assessor.", never a refusal.
- **No match:** "No request matches these filters." · what was asked ("Overdue only, with Fatima Khumalo.", "Overdue only.", "With Mohammed Patel.") · Clear filters.
- **Loading:** "Waiting" over three 3rem skeleton rows (`aria-busy`).
- **Failed:** StatePanel with `OnRetry`: "Could not load Waiting for assessors. Nothing has changed. Try again, or come back in a few minutes." with Try again, answering into the heading; a failed page turn or re-read after a reminder puts the focus on the failure.
- **A new question** (the filters change) clears the last reminder's answer: it was about the list as it was.

## Rules (DESIGN.md § Page-level patterns, "List page"; § Dashboard page, "The oversight Homes")

- **One read, one order, one set of words** with Home's Waiting for assessors card and the registrar page's section: oldest first by `UpdatedOn`, Overdue at 7 days, "With <name>" (E6).
- **A request is opened, never moved, from here.** No Reassign (T359); a reminder moves nothing.
- An activity opened from a waiting row lights Waiting for assessors for both speciality admins and the Coordinator, however it was reached (the owner table, NavMenu).

## Contrast

As WaitingList's: the link 6.70:1, the "since" line and the rule `muted-text` 5.09:1, "Overdue" 11.89:1 on `warning-bg` with its 5.43:1 edge; Send a reminder `secondary-color` 4.86:1 on the surface.

## Known gaps

- **The wait restarts on any save** (T351), which the rule line now says; a request an assessor saved yesterday reads "Less than a day" though it was filed weeks ago.
- **Reassign is not built** (T359): from here a request whose assessor cannot be reached can only be reminded about, and a refused reminder says why ("No reminder: Fatima Khumalo's account is deactivated.").
