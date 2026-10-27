# FormerTraineeHome

A former trainee's Home (`/`, `Pages/Home.razor`'s no-role branch; the card `Pages/Dashboards/FormerTraineeRecordCard.razor`, its words `FormerTraineeRecordWords.cs`, its read `Wombat.Application/Features/Dashboards/FormerTrainee/GetFormerTraineeHomeQuery.cs`): **one emphasised card, "Your training record"**, for a person who holds no role and keeps a trainee record (the `trainee_record` claim: a graduate, or a registrar who withdrew, T252, D54). Flow 13 (T379, 2026-10-27, 5cc10c66; boards R3-H01 to H08; Q1, Q23, Q25, F6, S8, S9) drew it. Until then the card was one sentence, "You hold no role at the moment. Your training record is kept, read-only: …", and "Open My progress →".

## Who reads it

A signed-in person with no role and the record's claim. A former trainee with a staff role keeps that role's Home and dashboard; their menu still offers My progress and Export portfolio (NavMenu). Anyone else with no role, and a record holder whose read finds no ended programme to show (a running profile, none, or an end with no recorded day), gets flow 01's "No role assigned" (`info`): "Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one." (R3-H04).

## The page

The header "Home", with no subtitle and no header action (flow 01's no-role frame). Under it, `DashboardFrame` (so the loading, H05, and the load error with Try again, H06, are every Home's), a `.dashboard-grid` with one `DashboardCard` (`Emphasis`, `Span="3"`, `book`, `#card-record`):

- **The end**, `p.ended-line` (1.1rem, 600): "You completed your programme on 2026-10-27." or "Your programme ended on 2026-10-25." (My progress's words for the end, `QuotaText.ProgrammeEnded`, so the two pages say it alike).
- "Your record is kept, read-only: no target applies to you any more."
- **The record's pages**, `ul.record-links`, one a row, the rows ruled apart (`border-color`, 8px padding), each its page's title as the link (600, with its icon) over `p.record-link-why` (muted, 0.9rem) that describes it, the link `aria-describedby` its line (Q23):

| Row | Link | Line |
|---|---|---|
| My progress | `trending-up` | "Every period of your programme, to training year 4, and your standing against Annexure A." (no training year where the record names none) |
| My authorisations | `award`, **only where a STAR was ever issued** (F6) | "Your 15 STARs in force, each with its certificate to download." ("Your 1 STAR in force, with its certificate to download."; with none in force now, "None of your STARs is in force now; My authorisations lists them.") |
| — with no STAR ever issued (R3-H02) | no link: `span.record-link-plain` "STARs" with `award` | `p.record-link-none` "No STAR was issued to you, so there is no certificate to download." |
| My activities | `list` | "Every activity you filed or that was filed about you, read-only." |
| Export portfolio | `download` | "A PDF of your record. Anyone holding it can check it on Verify portfolio export." |

The card's body is `.record-card-body`, a column 16px apart. No footer, no arrow, no count badge. The STARs are read as the programme left them (`RecordStars`, T312).

## States

- **Loading** (H05): the frame's skeleton card in the card's place, under Home.
- **Load failed** (H06): the frame's "**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes." with Try again (`refresh-cw`); the failure logged, never shown.

## Narrow widths

Below 641px each record link is a 44px target (`.record-links a`, 2.75rem; 24px from 641px, WCAG 2.5.8). The card is the column's width. Measured on the replay (wombat_scenario_t379, Lerato Molefe and Pieter du Plessis) at 1280 and 390: no sideways scroll, every link 44px at 390.

## Contrast

No new colour token. The lines `muted-text`, 5.09:1 on the surface; the rule `border-color`, decorative.

## Known gaps

- **Proposed** (T386): after Try again the focus lands on the card's region, not the h1 that R3-H06 draws it on.
