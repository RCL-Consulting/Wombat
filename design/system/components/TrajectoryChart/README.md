# TrajectoryChart

One EPA's rating trajectory (`Components/Shared/TrajectoryChart.razor`, drawn by `TrajectoryDrawing` and said by `TrajectoryWords`): a card with its heading and summary, a key, the chart drawn at two fixed sizes, and a table of the same ratings, always visible. The one rendering of `GetEpaTrajectoryForTraineeQuery`, for the trainee's EPA page and the committee's review page alike, so the chart is drawn and said one way on both. Flow 05 (T355, 2026-10-04, b020c942, with db4c2d0a, 5f8e6639 and b389f7d2 from its replay; Q5, E1, E2, R4; notes 10, 11, 13) rebuilt it. Until then it was T123's chart: one 600×200 drawing scaled to its card, 11px labels that shrank to about 5px at 390, and a table for screen readers only (T323).

## What the consumer provides

```razor
<TrajectoryChart Trajectory="_trajectory" HeadingLevel="2" Title="Rating trajectory" Today="@TodayOf(model)" MsfBeside="@HasMsf" />
<TrajectoryChart Trajectory="trajectory" HeadingLevel="4" ReviewWindow="true" SubjectName="@_review.TraineeName" />
```

- `Trajectory` (required): one EPA's ratings (`EpaTrajectoryDto`), with the window they were read over and the minimum's steps.
- `HeadingLevel` (default 2): the page's (note 10). 2 on the EPA page; on the committee page 4, one below its section's h3.
- `Title`: the heading's words where the page's h1 already names the EPA, "Rating trajectory", with the paused mark after it for a paused EPA. Null: the heading is the EPA's name through EpaLabel, "PAED-010 — Leading and operating within a clinical team".
- `Today`: draws the "Today" rule where it falls in the window. Null draws none: the committee page reads the review's window, not today's (decision D2).
- `ReviewWindow` and `SubjectName`: the committee page's summary names the trainee and the review's window. `SubjectName` alone (the registrar page, flow 06): the summary names the registrar and the academic year, even for one rating, "Nomsa Mahlangu · 1 rating in the 2026 academic year, from Thandi Zulu. At the minimum." ("1 rating so far" is the registrar's own phrase, for a chart of the registrar's own year).
- `MsfBeside`: the page has multi-source feedback beside the EPA, so the card says "Multi-source feedback is not plotted." (`p.trajectory-msf`) under the table.

## Markup

`section.detail-card.trajectory-card`, `id="trajectory-<EpaId>"`, named by its heading (`aria-labelledby="trajectory-<EpaId>-h"`), a column 16px apart:

- `div.trajectory-head`: the heading (`h2.epa-section-title` on the EPA page; a plain `h4` on the committee page), and the summary (muted): "3 ratings in the 2026 academic year, from David Naidoo, Thandi Zulu and Sarah Botha. 2 at the minimum, 1 below."; one rating, "1 rating so far, from David Naidoo. At the minimum."; on the committee page "Lerato Molefe · 3 ratings in the review window, 2026-01-01 to 2026-12-31, from Thandi Zulu, David Naidoo and Mohammed Patel. 2 at the minimum, 1 below." More than three assessors are counted, "from 4 assessors".
- `ul.trajectory-key`, named "How to read the chart": each mark's swatch (`aria-hidden`) beside its words: "Below the minimum: 3b until 2026-01-13, then 4 (training year 3)" (`--below`: `warning-bg` under a dashed `warning-color` top); "Exit level 5" (`--exit`, a 3px `primary-color` bar); "A rating" (`--dot`); "Rated on another scale: counts towards the number, not the level" (`--hollow`, a ring) only where a rating sits in the other lane.
- `div.trajectory-figure`: a named, focusable scroll region (`role="region"`, "Rating chart for PAED-001", `tabindex="0"`, the focus ring on `:focus-visible`) holding both drawings, `svg.trajectory-chart.trajectory-chart--sized.trajectory-chart--wide` (900 wide) and `…--narrow` (322 wide), each `role="img"` and named for what it adds to the table: "Chart of the 3 ratings in the table below."
- `div.table-container > table.clinic-table.clinic-table--stack.trajectory-table`, its caption "The ratings, oldest first" over it: Encounter as each row's header (`th scope="row"`, "2026-09-24", or the encounter's "not recorded (created …)" form), Rating ("4"; a rating on another ladder "Independent on O-R Scale"), Against the minimum then ("At or above (4, training year 3)", "Below (4, training year 3)", "Not on the ladder: counts towards the number, not the level", "No minimum"), Activity (its row name as My activities gives it, an `a.activity-link.activity-block-link`), Assessor.
- With no rating: the heading, then `p.card-empty` "No rating yet."

## Drawn twice, never scaled (Q5, E1)

Both drawings are in the markup, each at its own fixed geometry (`TrajectoryFrame.Wide`, `.Narrow`). **The card, not the viewport, chooses**: `.trajectory-figure` is a container (`container-type: inline-size`), and `@container (min-width: 900px)` shows the 900 drawing, the 322 below it, with no script. The EPA page's card holds 900px only from a viewport of about 1254px, so at 1100 it draws 322. The region is the floor for a card narrower than 322px: it scrolls the drawing sideways there and nothing else does.

- **One window on a real time axis:** month ticks, every month named at 900, every other at 322; every Semester 2 (July to December) banded on `header-bg`; each semester named above the plot ("Semester 1, 2026"). A window longer than about 14 months (a pre-graduation review's) names no month or semester: quarters at 900, and at 322 quarters where a quarter's name fits and years where it does not, the years above the plot where the axis names quarters, and no two names in a row overlap (build review G1). The EPA page reads the academic year containing today, or the one the programme ended in, and draws "Today" (a dotted `muted-text` rule); the committee page reads the review's window and draws none.
- **The ladder** top to bottom, a gridline through each rung (`border-color`, decorative); the area below the training year's minimum shaded `warning-bg` and edged by a stepped dashed `warning-color` line that steps where the training year changed; "Minimum 4" ("Min 4" at 322) and "Exit 5" beside the plot's right edge in `text-color`, never in the warning colour; the exit level a 2.5px `primary-color` line; a rating a filled `primary-color` dot ringed in the surface, joined oldest first by a 2px `secondary-color` line; a rating on another scale a hollow ring in its own lane under the rungs ("Other scale", "Other" at 322), left out of the line.
- Points closer than a dot are set a dot apart, in date order, and a run is set back from the right edge so no point sits on "Exit 5" or "Minimum 4"; a minimum step off the drawn ladder draws no minimum; no minimum before the programme starts.
- **Words:** rungs and months 13px in `text-color` (12px at 322), the rungs' names 600; the semester names, the lane's name and "Today" 13px muted (12px at 322); nothing under 12px.

**Below 641px** the card pads 16px, the table stacks (the encounter heads each block at weight 600 and wraps), and each activity link is a 44px block.

## On the three pages

- **The EPA page** (EpaPage): h2 "Rating trajectory", with `span.paused-mark` "(no longer in use)" after it for a paused EPA; the academic year with Today; "Multi-source feedback is not plotted." where the EPA's activities include MSF.
- **The registrar page** (`/programme/trainees/{id}`, flow 06, D8): a `section.list-section` titled by its h2 "Rating trajectories" holds a `.stack-list` of one card per EPA rated in the academic year (the one containing today, or the one the programme ended in), each headed by the EPA's name at h3, with no Today rule; empty, "No ratings yet in the 2026 academic year." ("No ratings in …" once ended). The standing panel's EPA names link to each card (`#trajectory-<EpaId>`).
- **The committee's review page** (`/committee/reviews/{id}`): a `section.list-section.full-width` titled by its h3 "Rating trajectory by EPA" spans the details grid, a `.stack-list` of one card per EPA with a rating in the review's window, each headed by the EPA's name at h4. The section is never a card around the charts (5f8e6639: a card in a card left each figure 874px at 1280, short of the 900 drawing); empty, "No rating by a named assessor to chart in this review's window." The standing panel's EPA names link to each chart's heading (EntrustmentStandingPanel).

## Rules (DESIGN.md § Badges, "Entrustment standing": the trajectory)

- The table says everything the drawing does; the drawing is named for what it adds.
- "Against the minimum then" is computed live with the comparer credit uses, at the training year of each encounter (E2), and by the activity's pinned credit directive: none where it names no minimum.
- Multi-source feedback is never plotted (D8, D36).
- Never scale the drawing; a new size is a new `TrajectoryFrame`.

## Contrast

The dashed minimum `warning-color` 5.77:1 on the surface, 5.43:1 on its own shading, 5.18:1 on the band; the exit line `primary-color` 10.98:1; the line `secondary-color` 4.86:1; the other lane's dotted rule `input-border` 3.80:1; the words `text-color` 12.63:1 and the muted names 5.09:1 on the surface (4.57:1 on the band); the shading and the gridlines are decorative.

## Known gaps

- The long-window axis (quarters, years) is not in this preview: the replay's states reached it on the pre-graduation review only.
