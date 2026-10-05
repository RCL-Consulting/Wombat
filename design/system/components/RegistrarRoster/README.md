# RegistrarRoster

Home's rows of registrars (`Components/Shared/Programme/RegistrarRoster.razor`): the first rows of Programme trainees, each name its own link to the registrar's page, then "3 more in Programme trainees." past them. Two readings: the Registrars card's (the name over the training year, then My progress's two figures in fixed columns) and Nothing filed in 30 days' (the name over when the registrar last filed). Flow 06 (T358, 2026-10-05, 85d5a508; Q1; R2-Home c1, c6, c11, k4; round 3 items 21, 27, 28, 33) made it.

## What the consumer provides

```razor
<RegistrarRoster Rows="@card.Rows" Total="@card.Total" />
<RegistrarRoster Rows="@summary.NothingFiled.Rows" Total="@summary.NothingFiled.Total" FiledMeta="true" />
```

- `Rows` (required): the list's first five, in its order (`ProgrammeTraineeRowDto`, read by Programme trainees' own reader, so a card and the page agree).
- `Total`: how many the list holds; past `Rows`, `p.waiting-more` "3 more in Programme trainees." (muted 0.9rem).
- `FiledMeta`: Nothing filed's rows.

The card's title, badge, rule line, empty words and foot are the Home's (RegistrarsCard; DashboardCard for Nothing filed).

## Markup

`ul.list-unstyled` of:

- **A roster row**, `li.roster-row`: a grid of `minmax(0, 1fr) 10rem 10rem`, `space-xs` by `space-lg` apart, padded `space-sm` top and bottom, ruled `border-color` from the row above (none above the first).
  - The first cell: `a.progress-row-link` (weight 600) "Nomsa Mahlangu", to `/programme/trainees/{ProfileId}`, over `p.progress-row-meta` "Training year 1" ("Programme not started").
  - Then two `.dashboard-metric`s: `span.count-figure` "0 of 10" over `.dashboard-metric-label` "EPAs met this semester"; "0 of 5" over "EPAs met in 2026" (`ProgrammeWords.SemesterFigure`, `YearFigure`, My progress's words; a window with no target is "none apply yet" over "Semester targets").
  - **An exempt registrar** has no figures: `div.roster-exempt` across both figure columns (`grid-column: 2 / -1`, a wrapping row): the badge "Exempt this period" (`badge-draft`) beside why, in words, "Starts on 2027-01-01" or "Started part-way through the period" (review 9).
- **A Nothing filed row**, `li.progress-row.progress-row--link` (the Trainee Home's link rows, the card being one column wide): the name's link over "Last filed 2026-09-12 · Training year 2" ("Nothing filed yet · Training year 1"); no figures.

**At 900px and below** a roster row is one column (`grid-template-columns: minmax(0, 1fr)`; `.roster-exempt` back to `grid-column: auto`), where the dashboard grid is: the figures fall under the name, so a figure never squeezes it from 641 to 900px (round 3 item 21; `StylesheetRuleTests.Flow06Homes`). Below 641px the name's link is a 44px block (`.progress-row-link`, flow 05).

## Rules (DESIGN.md § Dashboard page, "The oversight Homes"; § Dashboard layout grid, "The programme's figures")

- **Every row is its own link**, to the one registrar behind it; the card is never one link around them (T280).
- A registrar's figure is "n of m" over its window, never "n / m" or a percentage; ties by surname, then first name (T298).
- **No pronoun for a person** in any word (round 3 check 1): the registrar's name.
- The current registrars only (an active profile, an account that holds Trainee, not locked), so nothing here is "inactive" (Q10).

## Contrast

The name `link-color` 6.70:1; the meta lines and labels `muted-text` 5.09:1; a figure `text-color` at weight 600 (`.count-figure`), 12.63:1; the rule `border-color` (a hairline). The exempt badge 11.36:1 on `header-bg`, its edge 3.42:1.
