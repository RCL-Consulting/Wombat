# EpaPage

One EPA of the registrar's own curriculum (`/portfolio/progress/{EpaId:int}`, `Pages/Portfolio/EpaProgress.razor`): a record page under My progress, a personal link. What she has observed on it this period, her STAR, the rating trajectory, and every finished activity on it. Flow 05 (T355, 2026-10-04, b020c942; R2, R5; round 1, correction 2; E3, E4, C5, C8) made it; until then an EPA was a card on My progress and had no page. It admits whoever may open My progress (`TraineeOrFormerTrainee`), so a graduate reads each EPA of their record here.

## The page

- **Owned by My progress for every role, and for a graduate who holds none** (E3; `NavOwners.UnderAPersonalLink`): it lights My progress with `aria-current="true"` whatever the acting role, and its trail is Home › My progress › the code.
- **Addressed by the EPA's id, never its code.** An id that is no item of the caller's own curriculum, another institution's or an unknown one, is flow 01's Page not found, word for word ("There is no page at this address." / "Check the address, or start again from Home." / Go to Home), drawn in place, with no trail, so an id confirms nothing.
- **`PageHeader`**: the h1 is "<code> — <title>" (`TitleContent`: EpaLabel, with "(no longer in use)" in `span.paused-mark` for a paused EPA, and an institution's own EPA's neutral badge, "Kgosi Kgari Teaching Hospital's own"); the subtitle "3 a semester · Decided each semester · Exit level 5" (a local item "1 a year · Not in the College's exit rule"); the tab the h1's words, mark included, " · Wombat"; the crumb the code.
- **One header above every state** (C8): while it loads the h1 reads "Loading this EPA", the crumb "Loading…", and the status, always on the page, "Loading this EPA."; FocusOnNavigate focuses that h1 on arrival, and once loaded the same h1, now the EPA's, still has it. The skeleton is the page's shape (two cards in the reversed grid, then a card).
- **A paused EPA** has N6's info alert under the h1 (`div.epa-page-alert`): "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned are kept, and what is completed on it meanwhile is credited if it is restored."; no count and no MSF line.

## Its parts

`div.epa-stack`, a column 24px apart:

- **`div.details-grid.details-grid--reverse`** (2fr 1fr; one column at 900px and below):
  - **Observations** (`section.detail-card`, `h2#obs-h.epa-section-title`): `div.epa-figure-row`, the figure "3 of 3" (`.dashboard-metric-value`), "this semester", and its bar (at most 16rem, wrapping on a phone); `p.progress-row-meta` "Target met for Semester 2, 2026." or "2 more by 2026-11-30."; then `div.epa-lines`: "At the minimum level when observed: 2 of 3", "Last encounter 2026-09-28", the window before ("Semester 1, 2026: 0 of 3, 3 short"), the level line under a hairline (`p.epa-level-line`: "Training year 3: level 4, the minimum each encounter is judged against and your STAR's target."; KGK-001 "Minimum 3a"), and MSF's line whole ("MSF in Semester 2, 2026: no released campaign covering this EPA has closed yet. …").
  - **Entrustment** (`h2#ent-h`): the STAR from the standing, by EPA id: `div.star-level`, its level (`.dashboard-metric-value`) and its verdict badge; a `dl.details-list` of Issued, Expires (when it does) and Exit level ("5 · not yet"; a local EPA instead says "Not in the College's exit rule."); then "Open My authorisations" (`a.epa-auth-link`, a 44px block below 641px), shown only to a holder of the Trainee role, since My authorisations admits no one else (E4). With no STAR, "No STAR yet."; paused, "While PAED-012 is paused it is not in your standing against Annexure A."
- **The rating trajectory** (TrajectoryChart): h2 "Rating trajectory", over the academic year containing today, or the one the programme ended in, with Today; "Multi-source feedback is not plotted." where MSF sits beside the EPA.
- **Activities on this EPA** (`section.list-section`, `h2#acts-h.list-section-title`): a stacked table (Activity, State, Credit; its caption visually hidden, "Your finished activities on this EPA, newest encounter first"), each activity an `ActivityLink` with `Block` (`a.activity-link.activity-block-link`, a 44px block below 641px) with its "to <nominee>" line, its state's badge, and its credit ("1 item", "None", "—"); an activity whose EPA is paused is marked under the link (`span.activity-cell-paused`). Past the first page's size, "The newest 100 of 140 are listed." (`p.progress-row-meta`). Empty: the empty card "No activity on this EPA yet." with Log an activity (`.btn-outline`, 44px below 641px).

## Failures (T329, T272)

Every read is the caller's own record, and any that fails is the page's load error in fixed words, never its text: the h1 and the crumb "EPA", and "Could not load this EPA. Nothing has changed. Try again, or come back in a few minutes." with Try again, whose answer focuses the h1 (`PageFocus.FocusHeadingAsync`).

## Once the programme has ended (R5)

The page shows that EPA's periods the same way My progress's ended record does, every one read-only, with no figure, no bar and no "n more by" line; the chart is the academic year the programme ended in.

## Rules (DESIGN.md § Page-level patterns, "Record page under a personal link")

- No focus rule beyond the h1's: "File it again" lands on Log an activity's h1, the crumb back on My progress's.
- The ways in: Home's Furthest short rows and STAR rows, My progress's index and standing panel, the completed activity's "Open My progress", and My activities' Credit link ("1 item", named "1 item to PAED-001, in My progress"; `a.credit-link`, a 44px block below 641px).

## Known gaps

- The institution's own EPA's badge in the h1 is a category shown as a badge (EpaProgressTable, Known gaps) (T357).
- An activity on a paused EPA is marked under its link by `span.activity-cell-paused`, a third form of the paused mark beside `.paused-mark` and My activities' `.muted .text-sm` (EpaLabel, Known gaps) (T357).
