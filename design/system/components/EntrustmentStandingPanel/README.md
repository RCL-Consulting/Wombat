# EntrustmentStandingPanel

Where a trainee stands on each EPA of their curriculum (`Components/Shared/EntrustmentStandingPanel.razor`, T166): the active STAR decision against the level the curriculum sets for their training year (Annexure A's, for the College's EPAs) and against the exit level, beside the latest rating a named assessor gave. Shared by My progress and the committee's review page, so the two read the same table. Flow 05 (T355, 2026-10-04, b020c942, with db4c2d0a; C1, C7, C8, R4) changed it once for both pages and otherwise kept the built panel whole: the stacked table, the EPA name as a link where the page says, the rating as a 44px link at phone width, ISO dates (D1), the summary in `StandingWords`, and fixed words for a failure.

## What the consumer provides

```razor
<section class="standing-panel" aria-labelledby="standing-h">
  <h2 id="standing-h" tabindex="-1">Entrustment against Annexure A</h2>
  <EntrustmentStandingPanel Standing="@_standing" LoadError="@(_standingFailed ? EntrustmentStandingPanel.SelfLoadFailed : null)"
                            IsLoading="_standingLoading" OnRetry="RetryStandingAsync" FocusAfterRetry="FocusStandingAsync"
                            Self="true" ProgrammeEnd="@summary.Ended" EpaHref="EpaPage" />
</section>
```

- `Standing`: the `EntrustmentStandingDto`. Null says "You have no curriculum on record yet, so there are no targets to compare against." to the trainee (`Self`), and to anyone else "No standing to show: this trainee has no curriculum on record, or your account does not oversee their programme.", never "no curriculum" alone, since a caller outside the trainee's oversight gets the same null.
- `Self`: the trainee's own page: the sentences say "you" and "your".
- `ReviewPeriodTo` (the committee page): once the review period's last day has passed, the year is read for that day, and the opening sentence says so and that decisions are today's. `ProgrammeEnd` (My progress, ended): the year is read for the programme's last day.
- `EpaHref` (flow 05, R4): where each EPA's name links. My progress passes the EPA's page (`ProgressLinks.Epa`, `/portfolio/progress/{EpaId}`); the committee page passes that EPA's chart heading, `#trajectory-<EpaId>-h`, where the review window holds a rating, else null; the registrar page (flow 06) that EPA's chart card, `#trajectory-<EpaId>`, where the academic year holds a rating. Null for an EPA, or no `EpaHref`: the name is text.
- `IsLoading`, `LoadError`, `OnRetry`, `FocusAfterRetry`: the panel draws its states through StatePanel. Whatever `LoadError` carries, the words are fixed: with `Self` or `OnRetry`, "Could not load your standing. Nothing has changed. Try again, or come back in a few minutes." with Try again; otherwise "Could not load the trainee's standing. Nothing has changed. Reload the page, or come back in a few minutes." My progress answers Try again into `#standing-h`.

## Markup

The page gives the section and its heading (My progress: `section.standing-panel` with `h2#standing-h` "Entrustment against Annexure A"; the committee page: a `section.detail-card.full-width` with an h3 of the same words; the registrar page, flow 06: `section.standing-panel` with `h2#standing-h.epa-section-title`, `Self` false, `ProgrammeEnd` once ended, its own failure the section's alert with its own Try again, "Could not load the standing. Nothing has changed. Try again, or come back in a few minutes."). Inside:

- **The opening**, one C# sentence block: "You are in training year 3 on 2026-10-04. Each target is the level the curriculum sets for training year 3: Annexure A's, for the College's EPAs. Where it sets none, the EPA's exit level stands in, and the row says so. …" Before the programme starts it says the targets shown are training year 1's; on a review held after its period, that the period's last day falls in training year N; on an ended programme, that it ended, or was completed, then.
- `dl.details-list` of two: "STAR decisions against training year 3 targets", `StandingWords.YearLine`: "1 at or above · 0 below · 15 with no decision, of 16 EPAs" (Home's My authorisations card says the same); "Exit rule": "0 of 15 EPAs at their exit level by STAR decision (level 5: 0 of 9 · level 4: 0 of 6). Not yet: PAED-001, …. The institution's own EPA is not part of the College's rule, so it is not counted."
- `p.progress-row-meta`, directly under the rule: "For information only. Recording a Graduate decision or completing the programme does not check the exit rule."
- `div.table-container.shadow > table.clinic-table.clinic-table--stack.clinic-table--index`, its caption "Each EPA's STAR decision against the training year 3 target and the exit level, with the latest rating.", six columns:
  - **EPA**, the row header (`th scope="row"`, body weight, 34% wide): `a.epa-link` (weight 600, wrapping) by EpaLabel, or text; an institution's own EPA has "The institution's own EPA; not in the exit rule" under it.
  - **Year 3 target**: the level, "4"; where the curriculum sets none, the exit level with "Exit level; no year 3 level set" under it.
  - **STAR decision**: the level with "Issued 2026-10-04" under it (", expires …" when it does); "None".
  - **Against target**: a standing badge (`BadgeFor.Standing`): "At or above", "Below", "No decision", "Not comparable" with its reason under it ("Decided on O-R Scale, not CPSA Paediatric Entrustment Scale v11.1.", "Not on CPSA Paediatric Entrustment Scale v11.1.", "The curriculum sets no ladder for this EPA.").
  - **Exit level**: the level with "Reached", "Not yet" or "Not comparable" under it.
  - **Latest rating**: `a.standing-rating-link` "4 · Encounter 2026-09-28" (a visually hidden " — open this PAED-001 rating" after it) with its words under it, "Case analysis; at or above the target", "Direct observation; below the target"; a rating on another ladder "Independent on O-R Scale"; "None".
  Each cell's second line is `span.standing-rating-meta` (muted 0.85rem, a block).

The standing reads only EPAs in force (D48), so no row carries the paused mark.

**Below 641px** the table stacks (DataTable's Stack, written out): the EPA heads each block, every other cell is labelled by its column (`data-label`), the header row visually hidden but read; the EPA link and the rating link are 44px blocks, their two lines stacked.

## The in-page link (the committee page; build review A1, T342 A1)

An `EpaHref` that starts with `#` is a link in the page. Pressed, it moves the focus to the chart's heading (`PageFocus.FocusByIdAsync`) and navigates nowhere (`@onclick:preventDefault`): the app's `<base href="/">` would resolve a bare fragment against the site's root, and Blazor would navigate to Home and lose the page's unsaved decision. The `href` stays for a page with no circuit. It targets the heading, not the card (db4c2d0a), so the chart's summary is in view.

## Rules (DESIGN.md § Badges, "Entrustment standing")

- Every status is words in the cell, never a colour alone.
- Nothing here gates anything; the line under the exit rule says so.
- A level on another ladder, or on an item with no ladder, is "Not comparable" with its reason, never a verdict.
- A failure is fixed words, never the exception's text; the page beside it stands.

## Contrast

The badges as Badge's (words 11.23 to 11.89:1 on their tints); the second lines `muted-text` 5.09:1 on the surface; the links `link-color` 6.70:1.

## Known gaps

- An institution's own EPA is marked three ways across flow 05: under its name in this panel ("The institution's own EPA; not in the exit rule", text), as a neutral badge "Kgosi Kgari Teaching Hospital's own" in My progress's index and on the EPA page's h1 (EpaProgressTable, EpaPage), and in DESIGN.md's curriculum-list rule as "Your institution's own item" (`.muted .text-sm`) for anyone whose reads do not span institutions (T357).
- On the committee page the panel's EPA names link to charts in the page only where the review window holds a rating; KGK-001 and an EPA with no rating stay text, with nothing saying why.
