# EpaTargetCoverageList

Targets by EPA (`Components/Shared/EpaTargetCoverageList.razor`): each EPA's target for the current period as "n of m registrars met", fewest met first, each EPA's name a link to Programme trainees filtered Short on it. One component on every staff Home that has the card (the Committee member's and both speciality admins'), so they cannot drift. Flow 06 (T358, 2026-10-05, 85d5a508; Q5; R2-Home c1, c6, c8, c11; review 13; round 3 items 21, 31, 33) redrew it; until then it was "Targets met by EPA" on the committee's Home and "Curriculum coverage — Semester 2, 2026" on the admins', each EPA "Code — Title (3 per semester)" beside "4 of 9 met" over a named progressbar, no row a link.

## What the consumer provides

```razor
<DashboardCard Title="@TargetsByEpaWords.Title" Icon="book" HeadingId="card-epa-targets" Span="3" IsLoading="IsLoading">
  @if (Summary is { } summary) { <EpaTargetCoverageList Coverage="@summary.Coverage" /> }
</DashboardCard>
```

- `Coverage` (required): the programme's coverage for the current period (`CurriculumCoverage`, `CurriculumCoverageReader`): each EPA in force with how many current registrars it applies to and how many have met it, fewest met first, then by code; the exempt count.

The card is "Targets by EPA" (`book`, `#card-epa-targets`, spanning three) on every Home, never naming the semester, and has **no foot**: its list is Programme trainees, which the Registrars card opens.

## Markup and words (`TargetsByEpaWords`)

- **The rule line**, `p.needs-you-rule`: "Fewest registrars met first. Semester 2, 2026 ends on 2026-11-30."; with an exempt registrar, "1 registrar exempt this period, not counted. Fewest registrars met first. …".
- `ul.list-unstyled` of `li.coverage-row`: a grid of `minmax(0, 1fr) 10rem`, ruled from the row above as RegistrarRoster's.
  - **The name**: `a.progress-row-link` "PAED-002 — Managing common paediatric presentations", to `/programme/trainees?short=<EpaId>`, with the figure in words visually hidden after it (": 0 of 5 registrars met this semester. Show the registrars short on it."). **An EPA every registrar has met is text** (`span`), since nobody would be listed (item 33). Under it, `p.progress-row-meta`, its cadence: "3 per semester", "1 per academic year", then " · Kgosi Kgari Teaching Hospital's own" for an institution's own EPA, " · every registrar has met it" when all have.
  - **The figure**, a `.dashboard-metric` in its own column: `span.count-figure` "2 of 5" over `.dashboard-metric-label` "registrars met this semester" ("registrars met in 2026"), then the bar, `div.progress-bar[aria-hidden="true"]` with its fill (`.is-complete`, `success-color`, when all have met it): the words already say it, so the bar is hidden from a screen reader (DESIGN.md's one exception to the progressbar rule, item 31). **An EPA every registrar is exempt from** reads "All exempt" over "this period" with no bar, which would read as a coverage failure.
- **Empty**: `p.card-empty` "No targets this period: there is no current registrar."

At 900px and below a row is one column, the figure under the name, where the dashboard grid is (round 3 item 21).

## Rules (DESIGN.md § Dashboard layout grid, "Targets by EPA" and "The programme's figures")

- **A count, never a mean.** "2 of 5" says three have not; "40%" does not. The current registrars only (T238, T268), no "inactive" (Q10).
- Fewest registrars met first, then by code, so the EPA most short leads.
- **Every row is its own link** to the one list behind it, and the card is never one link around them (T280).

## Contrast

The name `link-color` 6.70:1; the cadence and caption `muted-text` 5.09:1; the fill on its `hover-bg` track `secondary-color` 4.61:1, complete `success-color` 5.58:1 (decorative here, `aria-hidden`, but held to 3:1 as every fill is).
