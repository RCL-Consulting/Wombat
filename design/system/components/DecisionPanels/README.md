# DecisionPanels

Decision panels (`/committee/panels`, `Pages/CommitteeDecisions/PanelsList.razor`; every sentence in `Panels/PanelWords.cs`): the panels that take entrustment and progression decisions at an institution, who sits on each, and **who decides each EPA**. Flow 08 (T364, 2026-10-13, 622c5ad1; boards R2-1; round 3 F2, M8, S6; C10, C11; T260, T329, T338) rebuilt it. Until then it was a bare table of names and counts, a scope printed as its enum, and a routing list that said there was no routing while it loaded.

## Who reads it

Committee member, Coordinator, Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`). The menu offers it to all six (NavMenu); it lights itself. A panel's own page admits only the four that manage panels (DecisionPanelPage), so the Committee member and the Coordinator read the panels here.

## The page, in order

`div.committee-calendar`, the wrapper flow 08's five pages share, to which every phone rule of the flow is scoped.

- **The header**: "Decision panels", the subtitle "The panels that take entrustment and progression decisions at Kgosi Kgari Teaching Hospital." ("…, by institution." for an Administrator who has chosen none). **New panel** (`btn-primary`, to `/committee/panels/new`) in the header only once the read is in and creating a panel would be accepted (`GetDecisionPanelFormOptionsQuery`, T256); never to a Committee member, a Coordinator or someone who holds Trainee.
- **An Administrator chooses the institution first** (F2; FilterBar): a GET form, `form.search-container[aria-label="Choose the institution"]`, Institution ("Select institution", then each institution) and Show; the address carries it (`?institution=4`). Until Show the list is the empty card "Choose an institution" · "An administrator belongs to no institution, so the panels are shown for the one chosen above." Show moves the focus to the list.
- **A manager not offered New panel is told why** (T260), in an info Alert: "You cannot create a decision panel. A speciality or sub-speciality administrator creates the panels of the speciality administered, …". Someone who holds Trainee reads the trainee note instead.
- **The list** (`#panels-list`, `tabindex="-1"`): a `.clinic-table--stack.clinic-table--stack-wide` captioned "Decision panels at Kgosi Kgari Teaching Hospital":

| Column | What |
|---|---|
| Name | the row header, the panel's name |
| Scope | in words, never the enum: "Speciality · Paediatrics", "Whole institution" (`PanelWords.ScopeCell`) |
| Decides for | the College committee it sits as, or "General panel" |
| Members | the count, then who sits by name as stored: "4: Thandi Zulu (chair), Sarah Botha, David Naidoo, John van Rensburg (external)" (M8) |
| (Actions) | Edit (`btn-outline btn-sm`), named for its panel, "Edit Paed Annual Review Panel", with what it decides for and its scope added where two names are alike (T239); only where the panel's page would open. A list where no row may be managed has no actions column. |

- **Who decides each EPA** (`section.panel-section`, `h2#routing-title`): the rule in a `.muted` paragraph ("Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi Kgari Teaching Hospital. …"), then per programme an h3 ("Paediatric EPA Curriculum 11.1") over a stack-wide table captioned "Who decides each EPA of Paediatric EPA Curriculum 11.1": EPAs (the row header) and Decided by. One row a distinct answer, its EPAs as ranges ("PAED-001 to PAED-003, PAED-006 to PAED-015"; two in a row read "PAED-004, PAED-005"), in the order of the first, each answered in one sentence: "Paed Annual Review Panel"; "Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the general panel decides."; "…, sitting as the …."; or, with no panel at all, every EPA in one row, "No panel at this institution covers this programme." It is read apart from the list.

## States (C11, T329, T338)

- **Loading**: the header at once; a visually hidden `role="status"` line, present from the first render, "Loading Decision panels."; three skeletons; Who decides each EPA's heading over two skeletons, **never a sentence saying there is no routing** (T329). No New panel.
- **Load failed**: "Could not load Decision panels. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel's `OnRetry`); the failure goes to the log. Who decides each EPA is not drawn.
- **Who decides each EPA's own failure**, in its section, the list standing: "Could not load who decides each EPA. Nothing has changed. Try again, or come back in a few minutes." with its own Try again. The Administrator's institution list failing: "Could not load the institutions. …" under the form.
- **Empty**: "No decision panels", with "Create a panel before scheduling reviews." for someone offered New panel, else "No decision panel runs at your institution yet." ("… at Kgosi Kgari Teaching Hospital yet." for an Administrator). Who decides each EPA still reads.
- An answer to an earlier read is dropped, so an Administrator switching institutions twice never sees one institution's panels under the other's name (T364 build review R1).

## Narrow widths

Both tables stack **at 900px and below** (`.clinic-table--stack-wide`, round 3 M8; DataTable), each row its row header over its labelled cells (`data-label`); the row header keeps the header cell's tint, which only the seat picker's drops (SeatPicker, build review D5). At 900px and below a stack-wide row's Edit is 44px (`.committee-calendar .clinic-table--stack-wide .btn`); below 641px every row action is 44px and the row's width, and so is New panel and the form's Show.

## Rules

- **Who sits is said by name**, never as a count alone; a scope is said in words.
- **New panel and Edit are offered exactly where the handler would accept them** (T256); a manager offered none is told why (T260).
- **No pronoun for a person**: the panel's members by name; "the chair".

## Contrast

No new colour token. Body text 12.63:1 and the routing rule (`muted-text`) 5.09:1 on the surface, 4.83:1 on the page; a row header on `header-bg` 11.36:1; Edit (`secondary-color` on the surface) 4.86:1; the info Alert's words 11.84:1 on `info-bg`.

## Known gaps

- None the replay met on this page (T329 and T260 no longer occur, Steps 2.18 to 2.32).
