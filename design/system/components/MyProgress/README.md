# MyProgress

My progress (`/portfolio/progress`, `Pages/Portfolio/MyProgress.razor`), a personal link: where the registrar stands this period, an index of every EPA, and the committee's standing. Flow 05 (T355, 2026-10-04, b020c942; R3, R4; Q1, Q5, C1, C8) redesigned it. Until then it was a grid of cards, one per EPA, each with its periods, its MSF line and its own rating chart, then the standing panel; the charts are now on each EPA's page (EpaPage). It admits the Trainee role or a trainee record (`TraineeOrFormerTrainee`), so a graduate reads their own record here.

## The page

- **`PageHeader`**: "My progress", with "Training year 3 · Semester 2, 2026" under it (`ProgressWords.Subtitle`; the semester alone with no training year; with no curriculum, the semester's name). No trail: it is a personal link, the current page.
- **The status**, `p.visually-hidden[role="status"]`, on the page from the first render: "Loading My progress." while it reads, empty once the read has answered, so its words are announced when they come (C8).
- **Notices**, info Alerts above This period: in December, "**The 2026 academic year ended on 2026-11-30.** Encounters observed in December still count towards semester 2, 2026 and your 2026 yearly targets. Semester 1, 2027 starts on 2027-01-01."; before the programme starts, "Your programme starts on …" with the first semester's and year's targets; started part-way, the waiver.
- **"This period"**, `section.detail-card.period-card` named by `h2#period-h` (`.dashboard-card-title`, `tabindex="-1"`): the two figures (`.dashboard-metric-row`; "1 of 10" over "EPAs met this semester", "0 of 6" over "EPAs met in 2026"), the semester's end (`p.period-ends`, muted 0.9rem: "Semester 2, 2026 ends on 2026-11-30."; in December "Semester 2, 2026 counts encounters observed in December."), then `div.period-lines` under a hairline: the training year ("Training year 3 — it sets the minimum level each encounter is judged against.") and MSF ("**Multi-source feedback:** 0 of 16 EPAs covered by a released campaign that closed this semester. MSF is tracked on its own and counts towards no target.", the term `span.period-line-term`, 600).
- **"Your EPAs"**, `section.index-section` under `h2#index-h`: the index (EpaProgressTable).
- **"Entrustment against Annexure A"**, `section.standing-panel` under `h2#standing-h` (`tabindex="-1"`): the standing panel drawn whole, each EPA's name linking to its EPA's page (EntrustmentStandingPanel).

With no trainee profile: the empty card "No curriculum items assigned yet." / "Once you are admitted to a curriculum, your EPAs will appear here."; with no EPA in force: "No EPA on your curriculum is in use at the moment, so no target applies to you." and the paused group alone.

## Failures (T329, T272; C8)

- **The page's read** fails into StatePanel with `OnRetry`: "Could not load your progress. Nothing has changed. Try again, or come back in a few minutes." with Try again; never the exception's text, which goes to the log. Try again's answer focuses This period's heading.
- **A section's own read** fails in place, and the rest of the page stands: MSF's as a line in This period with no button (`p.section-error`: "**Multi-source feedback:** Could not load MSF coverage. Your counts above are not affected."); the standing's as the panel's own alert with its own Try again, answering into `#standing-h`, while every verdict cell in the index reads "Not loaded".

## Once the programme has ended (R5)

The page keeps T252's record (flow 13 owns it), drawn since flow 06 by EndedRecord, the component the registrar page shares, byte for byte as before (`MyProgressTests`' golden markup): the subtitle "Your record of the programme you ended: what your curriculum held you to in each period.", an info Alert ("You completed your programme on 2026-10-04. This page is your record of it and is read-only: …"), an h2 "Curriculum targets" with a "Your programme" card (Started, Completed or Ended, Training year "4 when your programme ended"), and each EPA a card with every period read-only, newest first, its code a link to its EPA's page; then the standing as on the last day. Dates ISO (D1).

## Rules (DESIGN.md § Dashboard layout grid, "The progress figures")

- A figure is a count against a target for a named window; the words are `ProgressWords`, the same on Home, here and on the EPA page.
- "Training year N" for the stage and "Semester S, YYYY" for a quota window; never the bare word "year" beside both.
- The trajectories are on the EPA pages, not here.

## Known gaps

- The December notice says "towards semester 2, 2026", lower case, where every other window on the page is "Semester 2, 2026" (`ProgressWords.DecemberNotice`). December was not reachable in the replay (October), so the notice has not been seen on a screen.
- The ended record is still the pre-restructure card grid, under a heading, "Curriculum targets", that Home no longer uses (T311, T312; flow 13).
