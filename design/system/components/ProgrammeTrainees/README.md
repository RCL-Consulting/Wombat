# ProgrammeTrainees

Programme trainees (`/programme/trainees`, `Pages/Programme/ProgrammeTrainees.razor`): the current registrars in the scope of the role the page reads as, each a link to the registrar's page, with this semester's and this year's "n of m", furthest short and last filed, filtered with Show. Flow 06 (T358, 2026-10-05, 85d5a508, with 94edf2b7; R2-Trainees t1–t10; Q1, Q2, C6, C7; D3, D9; E4) made it, in the place of the placeholder stub flow 01 removed. It admits the Committee member, both speciality admins and the Coordinator, and is an item of each one's menu.

## The page

- **`PageHeader`**: "Programme trainees", with the scope under it once read: "Current registrars at Kgosi Kgari Teaching Hospital, read as Committee member · Semester 2, 2026" (an institution's scope, "at"); "Current registrars in Paediatrics, read as Sub-speciality admin · Semester 2, 2026" (a speciality's or sub-speciality's, "in"). No trail: the page is a list the menu offers, and lights itself.
- **The role it reads as** (`ProgrammeReadAs.RoleFor`, D2, E4): the acting role when the page admits it, else the first admitted role the person holds, in Home's order; never the union of the roles held. The subtitle names it. A role with no programme to read (no institution) gets flow 01's Page not found, drawn in place, which lights nothing (PageHeader's `Page`, NavMenu).
- **The status**, `p.visually-hidden[role="status"]`, from the first render: "Loading Programme trainees." while it reads.
- **The filters**, applied with Show (FilterBar): Short on (`select#f-epa`: "Any EPA", then each EPA in force as "PAED-002 — Managing common paediatric presentations"), Training year (`select#f-year`: "Any", then each year a current registrar is in), and Nothing filed in 30 days (a checkbox, its label once beside it); then Show and, once a filter is set, Clear filters. The form is named "Filter Programme trainees". Drawn while the list reads or has failed, and whenever there is a registrar; with none, no form.
- **The list**, `section.list-section` named by `h2#trainees-h.list-section-title` (`tabindex="-1"`): the heading counts the answer and takes the focus after Show, after a page turn and on arrival with a filter set (Home's EPA rows); then the rule line (`p.needs-you-rule`), the stacked table, and `PagerControls`, 20 a page, named "Current registrars, pages".

## The heading and the rule line (D9; `ProgrammeWords`)

| Asked | Heading | Rule line |
|---|---|---|
| Nothing | "5 current registrars" | "Fewest met first, then by surname. Semester 2, 2026 ends on 2026-11-30."; with an exempt registrar, "Fewest met first, then by surname. 1 registrar exempt this period, not counted, listed last." |
| Short on | "5 of 5 current registrars are short on PAED-002" | "PAED-002 — Managing common paediatric presentations, 3 per semester. Furthest from its target first, then fewest EPAs met, then by surname." |
| Training year | "2 of 5 current registrars are in training year 1" | as Nothing |
| Nothing filed | "5 of 5 current registrars have filed nothing in 30 days" | "Nothing filed (a draft is not filed) in the last 30 days; a recorded MSF counts. A registrar admitted less than 30 days ago is not listed. Longest without first." |
| Several | "1 of 5 current registrars matches these filters" | what was asked first ("Short on PAED-002, training year 4."), then the rule of the filter that sets the order |

Every count has its singular ("1 current registrar", "1 of 5 … is", "has"). No match is "0 of 2 current registrars" over the no-match card (below).

## The table

`table.clinic-table.clinic-table--stack.clinic-table--stack-wide.clinic-table--index` in a `.table-container.shadow`, its caption visually hidden ("Current registrars, fewest EPAs met first"; "Current registrars short on PAED-002, furthest from its target first"; "Current registrars with nothing filed in 30 days"; "Current registrars matching these filters"; ", one exempt this period" when any is). Each row is headed by the registrar (`th scope="row"`, `a.progress-row-link` to `/programme/trainees/{ProfileId}`), then Training year (the number, or "Not started"), then the columns the filter asks for:

- **The roster** (nothing, or Training year): This semester ("0 of 10" over "EPAs met this semester"), In 2026 ("0 of 5" over "EPAs met in 2026"), Furthest short ("PAED-002, PAED-003, PAED-005" over "0 of 3 each this semester"; one EPA, its own figure; different figures, "PAED-001: 1 of 3 this semester · PAED-010: 0 of 1 in 2026"; none short, "Every target met"), Last filed ("2026-09-27", or "Nothing filed yet").
- **Short on**: the EPA's own column first, headed by its code ("PAED-002": "0 of 3 this semester" over "3 more by 2026-11-30"), then This semester, In 2026 and Last filed.
- **Nothing filed**: Last filed first, then This semester and In 2026.

A figure is `span.count-figure` over `p.count-meta`; a window with no target is "none apply yet" over "Semester targets" ("Yearly targets"). **An exempt registrar** is listed last, its figure cells one cell (`colspan`) labelled "This period": the badge "Exempt this period" (`badge-draft`) over why, in words, "Starts on 2027-01-01" or "Started part-way through the period" (review 9). Every cell but the row's header carries `data-label`.

**It stacks at 900px and below**, not only below 641px (`.clinic-table--stack-wide`, 94edf2b7, A.7.8a): from 641 to 900px the sidebar leaves the column too narrow for six columns, and the names split mid-word (DataTable).

## Empty, no match, loading, failed

- **No current registrar** in the scope: under the heading "Current registrars", the empty card (`.detail-card--empty`) "No current registrars" · "They appear here once they are admitted to the programme.", and no form.
- **No match:** "No registrar matches these filters." · what was asked ("Nothing filed in 30 days.", "Short on PAED-002, training year 4.") · Clear filters (`btn-outline`, to the bare route): the one pattern Waiting for assessors shares (review 33).
- **Loading:** the filters stand; the heading "Current registrars" over four skeleton lines (`aria-busy`).
- **Failed:** StatePanel with `OnRetry`: "Could not load Programme trainees. Nothing has changed. Try again, or come back in a few minutes." with Try again, whose answer focuses the list's heading; a page turn that fails puts the focus on the failure. The exception's text goes to the log (T329).

## Rules (DESIGN.md § Page-level patterns, "List page"; § Dashboard layout grid, "The programme's figures")

- **Current** is an active profile on an account that still holds Trainee and that an administrator has not locked (`TraineeScopeResolver.KeepCurrentAsync`, T238, T268): an erased, locked or role-less profile is in no count and on no row. No "inactive" anywhere (Q10).
- A registrar's figure is "n of m" over its window; never "n / m", a percentage or the bare word "year". Ties by surname, then first name (T298).
- **No pronoun for a person** in any word the page writes (round 3 check 1): the registrar's name, or "the registrar".
- Home's Registrars, Targets by EPA and Nothing filed in 30 days are this list's previews, read by its own reader (`ProgrammeRosterReader`), so a card and the page cannot disagree.

## Contrast

Link `link-color` 6.70:1; the rule line, a figure's caption and the stacked labels `muted-text` 5.09:1; the exempt badge's words 11.36:1 on `header-bg`, its edge 3.42:1. Text pairs pass.

## Known gaps

- **The empty words use a pronoun**: "They appear here once they are admitted to the programme." (`ProgrammeWords.EmptyBody`, and Home's `RosterEmpty`), where round 3's check 1 allows none for a person. "Registrars appear here once admitted to the programme." would keep the rule.
- **The form is not a GET form**: Programme trainees' `form` has no `method="get"` and its fields no `name`, so before the circuit starts a press of Show reloads the bare list. Waiting for assessors' form names its fields with the query's keys and works with no circuit; DESIGN.md's rule ("a GET form above it") describes the second.
