# ProgrammeTraineeDetail

One registrar, read by a member of staff (`/programme/trainees/{ProfileId}`, `Pages/Programme/ProgrammeTraineeDetail.razor`): flow 05's parts in staff words, in one column of sections (This period, EPAs, the standing, the trajectories, what waits for an assessor, the committee reviews), each reading and failing on its own. Flow 06 (T358, 2026-10-05, 85d5a508; R2-Registrar r1–r8; C2, C8, C9; review 6, 7, 8, 30; D8) made it. It admits the Committee member, both speciality admins and the Coordinator, reached from Programme trainees and the oversight Homes by the trainee profile's id.

## The page

- **Read first, as the page's role** (`GetProgrammeTraineeQuery`, `ProgrammeReadAs`; C2, review 6): only the registrar's current profile, only in that role's scope, only while an account holds the user. Anything else is flow 01's Page not found, word for word ("There is no page at this address." / "Check the address, or start again from Home." / Go to Home), drawn in place, with the `search` icon and nothing lit (PageHeader's `Page`, NavMenu), so an id confirms nothing.
- **`PageHeader`**: the h1 is the registrar's name as stored, "Nomsa Mahlangu"; the tab "Nomsa Mahlangu · Wombat"; the subtitle "Training year 1 · Semester 2, 2026 · Kgosi Kgari Teaching Hospital, Paediatrics" (`RegistrarWords.Subtitle`: "Programme not started" for no year; "Programme completed 2026-10-03" or "Programme ended 2026-10-03" in place of the semester once ended). The trail is Home › Programme trainees › Nomsa Mahlangu, and Programme trainees is lit (`aria-current="true"`) for all four roles.
- **One header over every state** (r5, r8): while it reads and when its read fails, the h1 is "Programme trainee" with the same trail, so the h1 focused on arrival is the page's.
- **The status**, `p.visually-hidden[role="status"]`: "Loading this registrar's progress." while it reads.
- **The sections**, in `div.registrar-stack` (a column, `space-lg` apart; the stack owns the spacing, so `.index-section`'s, `.standing-panel`'s and `.list-section`'s own top margins are reset in it), each a `section` named by its `h2.epa-section-title` (`tabindex="-1"`):

| Section | What it holds |
|---|---|
| This period (`section.detail-card.period-card`, `#period-h`) | flow 05's two figures ("0 of 10" over "EPAs met this semester", "0 of 5" over "EPAs met in 2026"), then `p.period-ends` "Semester 2, 2026 ends on 2026-11-30. Training year 1 sets the minimum level each encounter is judged against." |
| EPAs (`section.index-section`, `#epas-h`) | EpaProgressTable with `EpaNamesAsText`: each EPA's name is text, never a link, since the EPA pages are the registrar's own (review 7); its STAR column from the standing. |
| Entrustment against Annexure A (`section.standing-panel`, `#standing-h`) | EntrustmentStandingPanel whole, `Self` false (no "you"); each EPA charted on this page is a link to its chart (`#trajectory-<EpaId>`), which takes the focus and navigates nowhere; any other is text. |
| Rating trajectories (`section.list-section`, `#trajectories-h`) | a `.stack-list` of TrajectoryChart, one per EPA rated in the academic year, each headed at h3 (`HeadingLevel="3"`), no Today rule (D8), its line naming the registrar: "Nomsa Mahlangu · 1 rating in the 2026 academic year, from Thandi Zulu. At the minimum."; none, "No ratings yet in the 2026 academic year." |
| Waiting for assessors (`section.detail-card`, `#waiting-h`) | the heading's badge in flow 04's words ("1 waiting, 1 overdue", `badge-submitted`); the emphasis stripe, or the warning one when any is overdue; the section's own `ActionResult`; the rule line "Oldest first. Overdue once it has waited 7 days. Its assessor is emailed after 5."; WaitingList's staff reading ("With Fatima Khumalo" over "Waiting 6 days"), each row ending in ReminderAction for a role that may remind (not the Committee member); empty, "Nothing of Nomsa Mahlangu's waits for an assessor." |
| Committee reviews (`section.detail-card`, `#reviews-h`) | each review the reader may open, newest first, a `.progress-row--link` row: "Annual progression review, 2026-11-20" (to `/committee/reviews/{id}`) over "Semester 2, 2026 · Scheduled" (the review's state label); none, "No review is scheduled for Nomsa Mahlangu." |

## An ended registrar (r6; round 3 check 2)

The subtitle "Training year 2 · Programme ended 2026-10-03 · …"; **EndedRecord** in staff words in place of This period and EPAs ("Pieter du Plessis's programme ended on 2026-10-03, part-way through Semester 2, 2026. No target applies after that; what follows is the record as it stood then, read-only."); the standing as on the last day; the charts over the academic year the programme ended in; and the waiting requests, still remindable.

## Failures (T329, T272; review 30)

- **The page's read** fails into StatePanel with `OnRetry`, under the h1 "Programme trainee": "Could not load this registrar's progress. Nothing has changed. Try again, or come back in a few minutes." with Try again, whose answer focuses the h1, now the registrar's name.
- **A section's own read** fails in its place and the rest stands: a danger alert (`div.alert.alert-danger.section-error`, `role="alert"`) with its lead in bold and the fixed rest, and its own Try again (`btn-outline btn-sm`, `refresh-cw`), which reads that section alone and focuses its heading: "**Could not load this period.**", "**Could not load the EPAs.**", "**Could not load the standing.**", "**Could not load the rating trajectories.**", "**Could not load what waits for an assessor.**", "**Could not load the committee reviews.**", each then "Nothing has changed. Try again, or come back in a few minutes.". While the standing has failed the EPAs' STAR cells read "Not loaded".
- **Loading:** the stack, `aria-busy`, with This period's, EPAs' and the standing's titles over skeleton lines.

## Rules (DESIGN.md § The NavMenu, the owner table; § Page-level patterns)

- **No pronoun for a person** in any word the page writes (round 3 check 1): the registrar's name, "the registrar" or "the assessor"; the reader is "you".
- **Staff words, the same figures.** The counts are flow 05's `ProgressWords`, the sentences around them `RegistrarWords`: nothing on the page says "your".
- **Each section reads apart and fails apart**, so a failure is that section's sentence, never the page's.
- A reminder sent here is answered in the section, and the rows read again (ReminderAction).

## Contrast

As the parts it draws (EpaProgressTable, EntrustmentStandingPanel, TrajectoryChart, WaitingList); a section error's words `text-color` on `danger-bg` 11.81:1, its edge 5.56:1.

## Known gaps

- **The ended record is still the pre-restructure card grid** under "Curriculum targets" (EndedRecord; flow 13), here as on My progress.
- **No way to the registrar's own EPA pages**: a member of staff reads the index with no per-EPA page of the registrar's, only the charts below (review 7, by design until a staff EPA page is designed).
