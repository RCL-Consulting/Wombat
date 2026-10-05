# EndedRecord

The record of an ended programme (`Components/Shared/Progress/EndedRecord.razor`): the notice, then every period of each EPA, read-only and newest first, with nothing about what is still to do. Two readers, one shape: My progress, in the registrar's own words, and the registrar page, in staff words. It is T252's record (flow 13's to redesign), extracted from My progress by flow 06 (T358, 2026-10-05, 85d5a508; review 8; round 3 check 2) so the registrar page could draw it; My progress renders the same bytes as before, held by `MyProgressTests`' golden markup. Dates ISO (D1).

## What the consumer provides

```razor
<EndedRecord Summary="_summary" Ended="ended" MsfCoverage="_msfCoverage" MsfFailed="_msfFailed" SubjectName="@registrar.Name" />
```

- `Summary` (required): the ended programme's counts, read as on its last day, every period back to its start.
- `Ended` (required): how the programme ended (`ProgrammeEndDto`: the day, if recorded, and whether it was completed).
- `MsfCoverage`, `MsfFailed`: each period's MSF coverage, a line on each card; failed, one line says so and the periods stand.
- `SubjectName`: the registrar, on the registrar page: the staff words, and no EPA-page links. Null on My progress.
- `FocusHeadingAsync()`: the record's heading, which a section's Try again focuses.

The standing as on the last day is the page's own section, after it (EntrustmentStandingPanel).

## Markup

- **The notice**, an info Alert with no role (standing content, there on every visit):
  - My progress: "You completed your programme on 2026-10-05. This page is your record of it and is read-only: no target applies to you any more. A period your programme ended in before that period's last month has no target, and no period after it is listed."
  - The registrar page: "Pieter du Plessis's programme ended on 2026-10-03, part-way through Semester 2, 2026. No target applies after that; what follows is the record as it stood then, read-only." ("Pieter du Plessis completed the programme on …" when completed; with no recorded day, "… programme has ended. Wombat did not record the day it ended, so the periods are shown up to 2026-10-05; what follows is the record, read-only.").
- **"Curriculum targets"**, an `h2` (`tabindex="-1"`; on the registrar page `h2#ended-h.epa-section-title` in `section.index-section`), then:
  - **The programme**, a `section.detail-card` headed `h3` "Your programme" ("The programme" for staff): a details list of Started ("2025-01-15"), Completed or Ended ("2026-10-03", or "The day was not recorded") and Training year ("2 when your programme ended"; "2 when the programme ended").
  - The MSF failure, when it failed: `p.section-error` "**Multi-source feedback:** Could not load MSF coverage. Your counts above are not affected." ("… The periods below are not affected." for staff).
  - **"Each semester"** and **"Once a year"** (`h3.progress-group-title`), each a `.dashboard-grid` of one `section.detail-card` per EPA: the code (`h3`; a link to the EPA's page on My progress, text on the registrar page), the title (muted), "Target: 3 per semester (6 a year). Minimum when your programme ended: 3a." (`p.progress-row-meta`; "when the programme ended" for staff), then a details list of every period, newest first: "Semester 2, 2026" · "no target (the programme ended part-way through) · 1 recorded"; "Semester 1, 2026" · "3 of 3, met; 2 at the minimum level when observed"; and its MSF line where there is one.
- With no EPA in use any more: `p.muted` "No EPA on your curriculum is in use any more, so there are no targets to show." ("… on Pieter du Plessis's curriculum …").

## Rules (DESIGN.md § Dashboard layout grid, "The progress figures")

- **A record, not a to-do list:** no bar and no "more by".
- **The staff words name the registrar** and never say "you" or "your" (`RegistrarWords`); the counted case has no person in it, so it is My progress's own words for both.
- The figures are the portfolio PDF's words for the same periods (`ProgressWords.EndedPeriodLine`).

## Contrast

The notice's words 11.84:1 on `info-bg`; the title and meta lines `muted-text` 5.09:1; terms 5.09:1 on the surface.

## Known gaps

- **Still the pre-restructure card grid**, under a heading, "Curriculum targets", that Home no longer uses; flow 13 redesigns it (T311, T312). On the registrar page its h2 is the section title while its EPA cards' codes are `h3`s beside the programme card's `h3`, one level for two kinds of heading.
