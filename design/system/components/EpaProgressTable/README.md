# EpaProgressTable

My progress's EPA index, "Your EPAs" (`Components/Shared/Progress/EpaProgressTable.razor`): one stacked table per group, each row an EPA whose name is its link to the EPA's page, its count for the window containing today, when a committee decides it, and its STAR against the training year's level, read from the standing by EPA id. Then "No longer in use", the paused EPAs, each with why it is not counted. Flow 05 (T355, 2026-10-04, b020c942; Q1, R3, C7, C13; round 1, correction 2) made it; until then My progress was a grid of cards, one per EPA, each carrying its periods, its MSF line and its rating chart.

## What the consumer provides

```razor
<section class="index-section" aria-labelledby="index-h">
  <h2 id="index-h">Your EPAs</h2>
  <EpaProgressTable Summary="summary" Standing="_standing" StandingFailed="_standingFailed" TrainingYear="VerdictYear" />
</section>
```

- `Summary` (required): the trainee's counts (`TraineeCurriculumProgressSummaryDto`): every item in force, by kind, and the paused ones.
- `Standing`: the standing, read by EPA id for the STAR column; null when there is none to read.
- `StandingFailed`: the standing's read failed: every verdict cell reads "Not loaded".
- `TrainingYear`: the year the STAR column is judged against, its heading "STAR against training year 3"; null, "STAR decision".
- `EpaNamesAsText` (flow 06, T358, review 7): every EPA's name, the paused group's included, is text (EpaLabel alone, no `a.epa-link`): the registrar page, where a member of staff reads someone else's index and the EPA pages are the registrar's own (ProgrammeTraineeDetail, section "EPAs"). False: My progress's links to each EPA's page.

The page wraps it in `section.index-section` (a column 24px apart, 24px under This period) under `h2#index-h` "Your EPAs". With every EPA paused (no item in force), My progress shows the empty card "No EPA on your curriculum is in use at the moment, so no target applies to you." and the index with the paused group alone.

## Markup

Each group a `div.table-container.shadow > table.clinic-table.clinic-table--stack.clinic-table--index` (explicit roles), semester first, as the College frames them (Annexure B); an empty kind draws none.

- **The caption is the group** (`caption.index-caption`, top, `text-color` 1.1rem/600, 16px padding): "Each semester · 10 EPAs", "Once a year · 6 EPAs". It is written `.clinic-table .index-caption` (0,2,0) to outrank the built `.clinic-table caption` (C7).
- **Columns:** EPA; the window ("Semester 2, 2026", "2026 academic year"); "Committee decides"; "STAR against training year 3".
- **The EPA** is the row header (`th scope="row"`, body weight, on the row's ground, 34% wide): `a.epa-link` (weight 600, wrapping) by EpaLabel, `/portfolio/progress/{EpaId}`; an institution's own EPA adds a neutral badge, "Kgosi Kgari Teaching Hospital's own" (`ProgressWords.LocalBadge`, `BadgeFor.State(Draft)`).
- **The count** (`td.count-cell`): `span.count-figure` (600) "1 of 3 this semester" / "0 of 1 in 2026", a bar at most 9rem (`aria-label` "PAED-002: 1 of 3 this semester"; `.is-complete` once met), and `p.count-meta` (muted 0.85rem): "2 more by 2026-11-30", "Target met for Semester 2, 2026."; in December, "1 more; encounters in December still count towards Semester 2, 2026.". A window with no target (waived, not started, or the programme ended part-way) is its one line in the figure's place, with no fraction and no bar: "No target this semester · 2 recorded · targets start with …", "No target yet".
- **Committee decides:** "Decided each semester", "Decided once a year", "Decided as opportunity allows"; an item with none (KGK-001) a muted dash (`span.cadence-none`, `aria-hidden`) and "No decision cadence" for a screen reader (notes 8, 9).
- **The STAR** (`td.verdict-cell`): the standing's badge ("At or above", "Below", "No decision", "Not comparable"), a space, then `span.verdict-level` (no wrap at desktop): "4", "4 · expires 2026-10-23", "Independent on O-R Scale"; with no STAR, the badge "No decision" alone; with the standing's read failed, "Not loaded".
- **"No longer in use · 1 EPA"**: columns EPA and "Why it is not counted"; each row `tr.is-paused` (both cells on `header-bg`), its name a link with the paused mark (EpaLabel), and "Paused by the College. It is not a target while it is paused. Its ratings and the credit it had earned are kept, and what is completed on it meanwhile is credited if it is restored." (`ProgressWords.PausedRow`). Its page still opens.

**Below 641px** each row is a block (DataTable's Stack): the EPA heads it with no padding of its own, the count, the cadence and the STAR labelled by their columns (`data-label`), the header row visually hidden but read; the caption pads 8px by 16px; the EPA link is a 44px block; the level may wrap.

## Rules (DESIGN.md § Dashboard layout grid, "The progress figures"; § Table system)

- A figure is a count against a target for a named window. Never "n / m", a percentage or a lifetime total; never the bare word "year" beside a training year and an academic year.
- The words are `ProgressWords`, the one class Home, My progress and the EPA page say a count in. Dates are ISO (D1).
- The bar never stands without its words (T280).
- A paused EPA is `tr.is-paused` with the paused mark, never `.detail-card--paused`, and is never counted (D48).
- The trajectories are on the EPA pages, not here (Q5).

## Contrast

The caption and figures `text-color` 12.63:1; `count-meta` and the dash `muted-text` 5.09:1; the paused row's mark 4.57:1 on `header-bg`; the bars as DashboardCard's (4.61:1, complete 5.58:1).

## Known gaps

- The institution's own EPA is a neutral badge here, a category shown as a badge, where DESIGN.md § Badges says a category is not a badge, and the standing panel and the curriculum lists mark the same thing as text under the title (EntrustmentStandingPanel, Known gaps). The badge also names the owner ("Kgosi Kgari Teaching Hospital's own") to the trainee, where the curriculum-list rule says "Your institution's own item" to anyone whose reads do not span institutions (T357).
- An ended programme's My progress is not the index: it keeps T252's record, each EPA a card with every period read-only (R5; flow 13 owns it).
