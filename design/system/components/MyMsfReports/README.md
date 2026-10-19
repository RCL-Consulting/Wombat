# MyMsfReports

The registrar's MSF reports: **her list** (`/msf/my-reports`, `Pages/MultiSourceFeedback/MyMsfReports.razor`) and **her report, a page of its own** (`/msf/my-reports/{id}`, `MyMsfReport.razor`); their words in `MyMsfReportText.cs`, where the coordinator's report says the same thing `MsfReportText`'s. Flow 10 (T369, 2026-10-19, 68327c9d; boards R3-M1 to M11; round 3 F3, M2, S6, S8; T164, T249, T269, T270, T280, T323, T325, T329) rebuilt them. Until then the list carried a Responses count (the campaign's total, which took in the suppressed groups), and a report opened in a card under the list, on the same page, with UTC times.

## Who reads it

The Trainee: the menu's "MSF reports" (NavMenu), which owns her report's page: Home › MSF reports › "Default MSF, released 2026-10-19", MSF reports lit. Both pages admit a Coordinator and an Administrator too (T269: the coordinator's report sends a campaign's subject here, whatever role brought them), but read them nothing that is not their own; a coordinator who is nobody's subject finds an empty list.

## The list

- **The header**: "MSF reports", the subtitle "Released aggregate feedback only. Individual responses are never shown."; no action.
- **"Your released reports"** (`h2.list-section-title`, in `.list-section.msf-measure`, 52rem): a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide` in a `.table-container.shadow`, captioned "Your released MSF reports, newest first", `scope="col"` on its headers: **Report**, the row's header (the template, "Default MSF", over its kind, `span.kind-line` "Multi-source feedback"); **Released** (`.nowrap`), "2026-10-19 13:43 SAST" (T325); and **View**, a small outline link named for its row, "View the Default MSF report, released 2026-10-19" (`RowNames.Distinct` where two would match, T239, T280).
- **No Responses column** (F3): `ListMsfCampaignsForTraineeQuery` returns `MsfReleasedReportDto`, which carries no count.
- **An id that is not one of her released reports** (not hers, not released, unknown) sends her here, `?unavailable=true`, replacing the address in the history: one info alert above the list, "That report could not be found. Your released reports are listed below.", which takes the focus as the page loads (`ActionResult FocusOnLoad`, M5, M11). An id confirms nothing. A failed read is never that alert (M10).

## Her report

- **The header**: h1, tab and crumb "Default MSF, released 2026-10-19"; the subtitle "Multi-source feedback · Released 2026-10-19 13:43 SAST". "MSF report" while read or failed. Shown only when the handler marks the copy hers (`IsSubjectsCopy`) and it is Released.
- **One column at the reading measure** (`.my-report`, 52rem, sections 24px apart):
  - **Narrative** (`section.detail-card.detail-card--emphasis`, `h2.section-title`), `p.narrative` keeping its line breaks; left out when none was given.
  - **What your colleagues said** (`h2`, `section.msf-stack-card`): `p.groups-intro` "Grouped by respondent role; no one is named. Ratings are averaged from 1, Well below expectations, to 5, Well above expectations. Comments are word for word." (learner feedback: "From the learners you taught; no one is named. …", and "Teaching contexts that responded: 2", never their names, T164); then the groups' cards, `MsfGroupCards ForTrainee`: **each shown group and its average, no count** ("Nurse · **4.33** average rating", T249), the question once, the comments verbatim (MsfReport).
  - **On your portfolio** (`section.detail-card`): "Recorded as evidence on 2026-10-19 13:43 SAST:" and each recorded EPA a link to its page under My progress (`ul.portfolio-links`, the title whole, M8); an EPA not recorded is left out; none: "This report records no evidence on your portfolio." (M4).

## States (T329)

- **Loading**: the list's heading over two row skeletons, said once ("Loading your MSF reports."); her report, two skeleton cards ("Loading the report.").
- **Load failed**: "Could not load your MSF reports. Nothing has changed. Try again, or come back in a few minutes." with Try again; her report, the report's fixed sentence with Try again.
- **None**: the empty card, "No released reports", "Released multi-source feedback will appear here.".

## Narrow widths

The list stacks at 900px and below (both stack classes; DataTable), View a 44px button from 900px and the row's width below 641px; her portfolio's links are 44px rows below 641px. Nothing scrolls sideways at 390.

## Contrast

No new colour token. As MsfReport's for the cards; the portfolio links 6.70:1; the kind line and the intro 5.09:1.

## Anonymity

Her copies print nothing of a group below the campaign's minimum (T249): not its label, its answers or its count, and the web copy prints no count at all, neither the total nor a group's. A report reaches her only once released, and its results only once the campaign has closed (F1). Comments are word for word, with no name.

## Known gaps

- **Needs confirmation (T371, proposed):** a group's comments are listed in the order they were submitted (MsfReport).
